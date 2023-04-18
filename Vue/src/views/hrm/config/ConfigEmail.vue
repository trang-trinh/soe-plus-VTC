<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";
import { encr, checkURL } from "../../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const toast = useToast();
const isFirst = ref(true);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const options = ref({
  IsNext: true,
  sort: " cp.approved_group_id DESC",
  sortDM: "card_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  pagenoDM: 0,
  pagesizeDM: 10,
  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
});

const danhMuc = ref();
const organization = ref();
const loadOrg = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_organization_get",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        organization.value = data[0];
      }

      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const listDropdownUser = ref();
const listUsers = ref([]);

const loadUser = () => {
  listUsers.value = [];
  listDropdownUser.value = [];
  axios
    .put(baseURL + "/api/hrm_config_email/update_data", null, config)
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        loadOrganization(store.getters.user.organization_id);
      } else {
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra vui lòng thử lại sau",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
    });
};

const loadOrganization = (value) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_config_email_get",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        config_email.value = data[0];
      }
    })
    .catch((error) => {
      options.value.loading = false;
    });
};
const config_email = ref({});

const editor = ref(ClassicEditor);
const editorConfig = ref({
  fontSize: {
    options: [8, 10, 12, 14, 16, 18, 20, 22, 24, 28, 32, 36, 40, 44, 48, 56],
  },
  toolbar: {
    items: [
      "heading",
      "bold",
      "italic",
      "underline",
      "Link",
      "|",
      "fontSize",
      "fontColor",
      "fontBackgroundColor",
      "fontFamily",

      "highlight",
      "|",
      "alignment",
      "bulletedList",
      "numberedList",
      "|",

      "insertImage",
      "mediaEmbed",
      "horizontalLine",
      "|",
      "insertTable",
      "tableColumn",
      "tableRow",
      "mergeTableCells",
      "|",

      "imageStyle:inline",
      "imageStyle:block",
      "imageStyle:side",
      "toggleImageCaption",
      "imageTextAlternative",
      "|",

      "strikethrough",
      "outdent",
      "indent",
      "|",
      "codeBlock",
      "linkImage",
      "blockQuote",
      "code",
      "subscript",
      "superscript",
      "|",
      "undo",
      "redo",
      "findAndReplace",
    ],

    shouldNotGroupWhenFull: true,
  },

  removePlugins: ["MediaEmbedToolbar"],
});

const saveDeConfig = () => {
 
  let formData = new FormData();
 
  formData.append("hrm_config_email", JSON.stringify(config_email.value));
   
  axios
    .put(
      baseURL + "/api/hrm_config_email/update_config_email",
      formData,
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật cấu hình gửi email thành công!");
        loadOrganization(store.getters.user.organization_id)
      } else {
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra vui lòng thử lại sau",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();

    });

}

onMounted(() => {
  loadOrg();
  loadUser();
  return {
    isFirst,
    options,
    danhMuc,
  };
});
</script>
<template>
  <div class="d-container p-0 w-full p-8 m-2 " style="padding:0 15% !important">
    <div class="form  formgrid p-0  w-full">
      <div class="col-12   p-0 flex align-items-center format-center">
<h3>CẤU HÌNH GỬI EMAIL</h3>
      </div>
      <div
        class="col-12 p-0 style-vb-2 text-center text-xl field"
        v-if="organization"
      >
        {{ organization.organization_name }}
      </div>
      <div class="col-12 field p-0 pt-4 flex align-items-center">
        <div class="col-6 p-0  ">
          <div class="w-10rem pb-2">Địa chỉ Email <span class="redsao pl-1"> (*)</span></div>
          <div >
            <InputText v-model="config_email.email_address" class="w-full" />
          </div>
        </div>
        <div class="col-6 p-0  pl-2">
          <div class="w-10rem pb-2">Mật khẩu Email <span class="redsao pl-1"> (*)</span></div>
          <div >
            <Password
            style="cursor: pointer; width: 33.33%"
            v-model="config_email.email_pasw"
            autocomplete="new-password"
            class="w-full"
            toggleMask
          >
            <template #header>
              <h6>Chọn mật khẩu</h6>
            </template>
            <template #footer="sp">
              {{ sp.level }}
              <Divider />
              <p class="mt-2">Gợi ý</p>
              <ul class="pl-2 ml-2 mt-0" style="line-height: 1.5">
                <li>Có ít nhất 1 chữ thường</li>
                <li>Có ít nhất 1 chữ hoa</li>
                <li>Có ít nhất 1 ký tự số</li>
                <li>Tối thiểu 8 ký tự</li>
              </ul>
            </template>
          </Password> 
          </div>
        </div>
      </div>
      <div class="col-12 field p-0 flex align-items-center">
        <div class="col-6 p-0  ">
          <div class="pb-2" >Incoming mail server</div>
          <div >
            <InputText v-model="config_email.incoming_mails" class="w-full" />
          </div>
        </div>
        <div class="col-6 p-0  pl-2">
          <div class="pb-2" >Out going mail server(SMTP)</div>
          <div  >
            <InputText v-model="config_email.outgoing_mails" class="w-full" />
          </div>
        </div>
      </div>
      <div class="col-12 field p-0 flex align-items-center">
        <div class="col-6 p-0  ">
          <div class="pb-2">Port</div>
          <div  >
            <InputText v-model="config_email.port" class="w-full" />
          </div>
        </div>
        <div class="col-6 p-0 pl-2 ">
          <div class="pb-2 ">Time set</div>
          <div  >
            <InputNumber v-model="config_email.time_set" class="w-full" />
          </div>
        </div>
      </div>
      <div class="col-12 field p-0 flex align-items-center">
        <div class="col-6 p-0  ">
          <div class="pb-2" >Tên hiển thị </div>
          <div  >
            <InputText v-model="config_email.display_name" class="w-full" />
          </div>
        </div>
        <div class="col-6 p-0 pl-2  ">
          <div  class="pb-2">Url link nội dung</div>
          <div >
            <InputText v-model="config_email.url_content" class="w-full" />
          </div>
        </div>
      </div>
      <div class="col-12 field p-0   align-items-center">
        <div class="pb-2">Nội dung mặc định</div>
          <div>
            <ckeditor
                :editor="editor"
                :config="editorConfig"
                v-model="config_email.default_content"
         
              ></ckeditor>
          </div>
      </div>
      <div class="col-12 style-vb-3 py-4 text-center format-center">
          <Button @click="saveDeConfig()">Cập nhật</Button>
        </div>
    </div>
  </div>
</template>
<style scoped>
 

@media only screen and (max-height: 768px) {
  .style-vb-1 {
    font-size: large !important;
    padding: 8px !important;
  }

  .style-vb-2 {
    font-size: small !important;
  }

  .style-vb-3 {
    padding: 8px !important;
  }
 
}

@media only screen and (max-height: 678px) {
  .style-vb-5 {
    display: none;
  }

 
}
</style>