<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import configUsers from "./ConfigUserCode.vue";
import ConfigContract from "./ConfigContract.vue";
import ConfigHolidays from "./ConfigHolidays.vue";
import ConfigInsurance from "./ConfigInsuranceRate.vue";
import ConfigEmail from "./ConfigEmail.vue";
import { encr, checkURL } from "../../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const toast = useToast();

const expandedKeys = ref([]);

const selectedUser = ref([]);

const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const checkMultile = ref(false);

const isFirst = ref(true);

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const options = ref({
  IsNext: true,
  sort: "   DESC",
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
const onTabOpen=(event)=>{


  activeIndex.value=event.index;
 
}
const activeIndex=ref(0);
onMounted(() => {
  loadOrg();
  return {};
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-0">
    <div class="d-lang-table surface-0">
      <div class="w-full p-2 py-3 style-vb-1 font-bold text-center text-3xl">
      THIẾT LẬP CÁC THAM SỐ HỆ THỐNG
      </div>
      <div
        class="w-full p-0 style-vb-2 text-center text-xl field"
        v-if="organization"
      >
        {{ organization.organization_name }}
      </div>
      <div style=";overflow-y: scroll; height: calc(100vh - 150px);">
      <div style="margin: 0 12%">
        <Accordion :multiple="false" :activeIndex="activeIndex"  @tab-open="onTabOpen($event)">
          <AccordionTab >
            <template #header>
                <div class="text-xl">
                    Mã nhân sự
                </div>
            </template>
            <div  v-if="activeIndex==0" ><configUsers /></div>
          </AccordionTab>
          <AccordionTab  >
            <template #header>
                <div class="text-xl">
                    Số hợp đồng lao động
                </div>
            </template>
            <div   v-if="activeIndex==1"   ><config-contract /></div>
          </AccordionTab>
          <AccordionTab  >
            <template #header>
                <div class="text-xl">
                  Tính số ngày phép trong năm
                </div>
            </template>
            <div class="check-scroll-2"   v-if="activeIndex==2">
              <ConfigHolidays />
              </div>
          </AccordionTab>
          <AccordionTab  >
            <template #header>
                <div class="text-xl">
               Tỷ lệ đóng bảo hiểm
                </div>
            </template>
            <div    v-if="activeIndex==3">
              <ConfigInsurance />
              </div>
          </AccordionTab>
          <AccordionTab  >
            <template #header>
                <div class="text-xl">
           Cấu hình gửi Email
                </div>
            </template>
            <div    v-if="activeIndex==4">
              <ConfigEmail />
              </div>
          </AccordionTab>
          
        </Accordion>
      </div></div>
    </div>
  </div>
</template>
<style scoped>
.d-lang-table {
  margin: 8px;
  height: calc(100vh - 50px);
}

.check-scroll {
  max-height: 50rem;
  overflow-y: scroll;
  overflow-x:hidden;
}
.check-scroll-1 {
  max-height: 45rem;
  overflow-y: scroll;
  overflow-x: hidden;
}
.check-scroll-2 {
  max-height: 41rem;
  overflow-y: scroll;
  overflow-x: hidden;
}
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

  .check-scroll {
    max-height: 25rem;
    overflow: scroll;
  }
}

@media only screen and (max-height: 678px) {
  .style-vb-5 {
    display: none;
  }

  .check-scroll {
    max-height: 40rem;
    overflow: scroll;
  }
}
</style>