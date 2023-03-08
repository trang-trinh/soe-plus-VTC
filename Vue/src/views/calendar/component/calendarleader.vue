<script setup>
import { onMounted, inject, ref } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import treeuser from "../../../components/user/treeuser.vue";

const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const isDynamicSQL = ref(false);
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
});

//Declare
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const options = ref({
  loading: false,
});
const submitted = ref(false);
const selectedNode = ref([]);
const datas = ref([]);

//Function
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const selectedUser = ref([]);
const is_one = ref(false);
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const showModalUser = (one) => {
  selectedUser.value = JSON.parse(JSON.stringify(datas.value));
  headerDialogUser.value = "Chọn lãnh đạo";

  is_one.value = one;
  displayDialogUser.value = true;
  forceRerender();
};
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
const choiceUser = () => {
  var notexist = selectedUser.value.filter(
    (a) => datas.value.findIndex((b) => b["user_id"] === a["user_id"]) === -1
  );
  if (notexist.length > 0) {
    datas.value = datas.value.concat(notexist);
  }

  closeDialogUser();
};

//function
const deleteItem = (item, index) => {
  datas.value.splice(index, 1);
};
const saveModel = () => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  let formData = new FormData();
  let users = datas.value.map((x) => x.user_id);
  formData.append("users", JSON.stringify(users));
  axios
    .put(baseURL + "/api/calendar_week/update_leader", formData, config)
    .then((response) => {
      if (response.data.err === "1") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      swal.close();
      toast.success("Cập nhật thành công!");
      props.closeDialog();
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
  if (submitted.value) submitted.value = true;
};

//initData
const initData = (ref) => {
  if (ref) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_leader_get",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            datas.value = tbs[0];
          } else {
            datas.value = [];
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
onMounted(() => {
  initData(true);
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '40vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <Toolbar class="outline-none surface-0 border-none p-0 pb-3">
            <template #start> </template>
            <template #end>
              <Button
                @click="showModalUser(false, 0)"
                label="Thêm lãnh đạo"
                icon="pi pi-plus"
              />
            </template>
          </Toolbar>
        </div>
        <div class="col-12 md:col-12">
          <DataTable
            :value="datas"
            :scrollable="true"
            :lazy="true"
            :rowHover="true"
            :showGridlines="true"
            v-model:selection="selectedNode"
            dataKey="leader_id"
            scrollHeight="flex"
            filterDisplay="menu"
            filterMode="lenient"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            responsiveLayout="scroll"
          >
            <Column
              field="avatar"
              header="Họ và tên"
              class="align-items-center justify-content-left text-left"
              headerStyle="text-align:left;min-width:100px"
              bodyStyle="text-align:left;min-width:100px"
            >
              <template #body="slotProps">
                <div class="flex">
                  <div class="format-center">
                    <Avatar
                      v-bind:label="
                        slotProps.data.avatar
                          ? ''
                          : (slotProps.data.last_name ?? '').substring(0, 1)
                      "
                      v-bind:image="basedomainURL + slotProps.data.avatar"
                      :style="{
                        background: bgColor[slotProps.index % 7],
                        color: '#ffffff',
                        width: '3rem',
                        height: '3rem',
                        fontSize: '1.4rem !important',
                      }"
                      class="mr-2 text-avatar"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                  <div>
                    <div>{{ slotProps.data.full_name }}</div>
                    <div class="description">
                      {{ slotProps.data.position_name }}
                    </div>
                    <div class="description">
                      {{ slotProps.data.role_name }}
                    </div>
                  </div>
                </div>
              </template>
            </Column>
            <Column
              header="Chức năng"
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:100px;"
              bodyStyle="text-align:center;max-width:100px;"
            >
              <template #body="slotProps">
                <Button
                  @click="deleteItem(slotProps.data, slotProps.index)"
                  class="p-button-rounded p-button-secondary p-button-outlined"
                  type="button"
                  v-tooltip.top="'Xóa'"
                  icon="pi pi-trash"
                ></Button>
              </template>
            </Column>
            <template #empty>
              <div
                class="block w-full h-full format-center"
                v-if="!datas || datas.length === 0"
              >
                <h3 class="description">Không có dữ liệu</h3>
              </div>
            </template>
          </DataTable>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveModel()" />
    </template>
  </Dialog>
  <!--treeuser-->
  <treeuser
    :key="componentKey"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="is_one"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
</template>
<style scoped>
.description {
  color: #999;
  font-size: 12px;
}
</style>
