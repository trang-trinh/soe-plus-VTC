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
    .put(baseURL + "/api/hrm_config_contact/update_data", null, config)
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
            proc: "hrm_config_contact_get",
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
        config_contact.value = data[0];
        debugger
      }
    })
    .catch((error) => {
      options.value.loading = false;
    });
};
const config_contact = ref({});

const saveDeConfig = () => {
  let formData = new FormData();

  formData.append("hrm_config_contact", JSON.stringify(config_contact.value));

  axios
    .put(
      baseURL + "/api/hrm_config_contact/update_config_contact",
      formData,
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật cấu hình danh bạ thành công!");
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
  <div class="d-container p-0 w-full p-8 m-2" style="padding: 0 15% !important">
    <div class="form formgrid p-0 w-full">
      <div class="col-12 p-0 flex align-items-center font-bold">Danh bạ</div>
    </div>
    <div class="col-12 field p-0 pt-4 flex align-items-center">
      <div class="col-6 p-0">Không hiển thị ngày sinh của cá nhân</div>
      <div class="col-6 p-0">
        <InputSwitch
          class="w-4rem lck-checked"
          v-model="config_contact.is_date_of_birth"
        />
      </div>
    </div>
    <div class="col-12 field p-0  flex align-items-center">
      <div class="col-6 p-0">Không hiển thị số điện thoại của cá nhân</div>
      <div class="col-6 p-0">
        <InputSwitch
          class="w-4rem lck-checked"
          v-model="config_contact.is_phone_number"
        />
      </div>
    </div>
    <div class="col-12 style-vb-3 py-4 text-center format-center">
      <Button @click="saveDeConfig()">Cập nhật</Button>
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