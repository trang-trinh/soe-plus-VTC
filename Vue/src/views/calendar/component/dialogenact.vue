<script setup>
import { ref, inject, onMounted } from "vue";
import moment from "moment";
import { socketMethod } from "../../../util/methodSocket";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { de } from "date-fns/locale";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();

//Declare
const submitted = ref(false);

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  modelenact: Object,
  files: Array,
  selectFile: Function,
  removeFile: Function,
  selectedNodes: Array,
  initData: Function,
});

//Function
const enact = () => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var obj = { ...props.modelenact };
  var calendars = props.selectedNodes.map((x) => x["calendar_id"]);

  let formData = new FormData();
  formData.append("content", obj["content"]);
  formData.append("read_date", obj["read_date"]);
  formData.append("calendars", JSON.stringify(calendars));
  for (var i = 0; i < props.files.length; i++) {
    let file = props.files[i];
    formData.append("files", file);
  }
  axios
    .put(baseURL + "/api/calendar_week/enact_calendar", formData, config)
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
      props.initData(true);
      swal.close();
      toast.success("Ban hành thành công!");
      props.closeDialog();
      if (response.data.data != null) {
        var datas = JSON.parse(response.data.data);
        if (datas != null && datas.length > 0) {
          datas.forEach((item) => {
            socketMethod
              .post("sendnotification", {
                uids: item["uids"],
                options: {
                  title: item["title"],
                  text: item["text"],
                  image:
                    baseURL +
                    (store.getters.user.background_image ||
                      "../assets/background/bg.png"),
                  tag: "project.soe.vn",
                  url: "/calendar/detail/".concat(item["calendar_id"]),
                },
              })
              .then((res) => {});
          });
        }
      }
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
  if (submitted.value) submitted.value = false;
};
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '40vw' }"
    :maximizable="false"
    :closable="false"
    style="z-index: 1001"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Nội dung</label>
            <Textarea
              v-model="props.modelenact.content"
              :autoResize="true"
              rows="5"
              cols="30"
            />
          </div>
        </div>
        <div class="col-12 md-col-12">
          <div class="form-group">
            <label>Tệp đính kèm</label>
            <FileUpload
              :multiple="false"
              :fileLimit="1"
              :show-upload-button="false"
              :show-cancel-button="true"
              @remove="props.removeFile"
              @select="props.selectFile"
              name="demo[]"
              url="./upload.php"
              accept=""
              choose-label="Chọn tệp"
              cancel-label="Hủy"
            >
              <template #empty>
                <p>Kéo thả tệp đính kèm vào đây.</p>
              </template>
            </FileUpload>
          </div>
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
      <Button label="Gửi" icon="pi pi-send" @click="enact()" />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(./stylecalendar.css);
</style>
<style lang="scss" scoped>
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-table {
    position: absolute;
  }

  .p-datatable-thead {
    position: sticky;
    top: 0;
    z-index: 1;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
  }
  .p-avatar-text {
    font-size: 1rem;
  }
}
::v-deep(.avatar-item) {
  .p-avatar.p-avatar-lg {
    width: 3rem;
    height: 3rem;
  }
}
::v-deep(.is-close) {
  .p-panel-header {
    color: red;
  }
}
</style>
