<script setup>
import { ref, defineProps, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { formatDate } from "../../util/function";
import { encr } from "../../util/function";
const cryoptojs = inject("cryptojs");
const props = defineProps({
  displayDocConnectSend: Boolean,
  key: Number,
  closeDialog: Function,
  checkedDocs: Array,
});
const toast = useToast();
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
// Defined Variable
const isLoading = ref(false);
const connect_item = ref({ type_export: "0" });
const headerDocConnectSend = ref();
const connect_orgs = ref([]);
const loadConnectOrgs = () => {
  isLoading.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_connect_list_organization",
            par: [
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        connect_orgs.value = data;
      }
      isLoading.value = false;
      swal.close();
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const sendDocConnect = () => {
  if (connect_orgs.value.filter((x) => x.checked).length === 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Chưa chọn đơn vị nào !",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  let checked_items = connect_orgs.value.filter((x) => x.checked);

  let formData = new FormData();
  formData.append(
    "connect_orgs",
    JSON.stringify(checked_items.map((x) => x.organization_connect_id)),
  );
  formData.append(
    "send_docs",
    JSON.stringify(props.checkedDocs.map((x) => x.doc_master_id)),
  );
  formData.append("type_export", connect_item.value.type_export);
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "post",
    url: baseURL + `/api/DocConnect/Send_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      debugger;
      if (response.data.err != "1") {
        swal.close();
        toast.success("Gửi văn bản thành công!");
        props.closeDialog();
      } else {
        swal.fire({
          title: "Thông báo",
          text: "Xảy ra lỗi khi gửi văn bản.",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
onMounted(() => {
  headerDocConnectSend.value = "Gửi văn bản lên trục liên thông";
  loadConnectOrgs();
  return {
    isLoading,
  };
});
</script>
<template>
  <Dialog
    :modal="true"
    :header="headerDocConnectSend"
    :visible="displayDocConnectSend"
    :autoZIndex="true"
    :style="{ width: '50vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <DataTable
              :value="connect_orgs"
              responsiveLayout="scroll"
            >
              <Column>
                <template #body="slotProps">
                  <Checkbox
                    :binary="true"
                    v-model="slotProps.data.checked"
                  />
                </template>
              </Column>
              <Column
                field="organization_connect_id"
                header="Mã đơn vị"
              ></Column>
              <Column
                field="organization_connect_name"
                header="Tên đơn vị"
              ></Column>
            </DataTable>
          </div>
        </div>
        <div class="field col-12 md:col-12 mt-5">
          <div class="col-12 md:col-12 m-0 p-0 flex">
            <label><b>Định dạng file dữ liệu xuất ra</b></label>
          </div>
          <div
            class="col-12 md:col-12 m-0 p-0 flex mt-3"
            style="gap: 1rem"
          >
            <RadioButton
              name="type_xml"
              value="0"
              v-model="connect_item.type_export"
            />
            <label
              for="type_xml"
              class="mt-1"
              >XML</label
            >
            <RadioButton
              name="type_json"
              value="1"
              v-model="connect_item.type_export"
            />
            <label
              for="type_json"
              class="mt-1"
              >JSON</label
            >
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-text"
      />
      <Button
        label="Gửi"
        icon="pi pi-check"
        @click="sendDocConnect()"
      />
    </template>
  </Dialog>
</template>
<style scoped lang="scss">
::v-deep(.p-panel) {
  .p-panel-header {
    border: none;
    width: max-content;
  }
  .p-panel-content {
    border: none;
  }
}
.recall-message {
  color: red;
}

.message-text {
  white-space: pre-wrap;
}
.message-wrap {
  overflow-y: auto;
  max-height: calc(100vh - 12.5rem) !important;
}
.message-file {
  display: flex;
  column-gap: 1rem;
  flex-wrap: wrap;
  row-gap: 1rem;
}
.file-box {
  max-width: 200px;
  border: 1px solid aliceblue;
  background-color: aliceblue;
  padding: 1rem;
}
.file-box a {
  display: flex;
  flex-direction: column;
  align-items: center;
}
.file-title {
  color: #333;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 170px;
  overflow: hidden;
  display: inline-block;
}
.file-box:hover .file-title {
  color: #2196f3;
}
</style>
