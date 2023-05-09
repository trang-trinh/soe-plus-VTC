<script setup>
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function";
import moment from "moment";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  profile_id: String,
});

//Declare
const insurance_status = ref([
  { status: 1, title: "Trả" },
  { status: 2, title: "Sửa" },
  { status: 3, title: "Chốt" },
  { status: 4, title: "Xin cấp" },
  { status: 5, title: "Gộp" },
  { status: 6, title: "Người lao động giữ sổ" },
]);
const insurance = ref({});
const insurance_pays = ref([]);
const insurance_resolves = ref([]);
function formatNumber(a, b, c, d) {
  var e = isNaN((b = Math.abs(b))) ? 2 : b;
  b = void 0 == c ? "," : c;
  d = void 0 == d ? "," : d;
  c = 0 > a ? "-" : "";
  var g = parseInt((a = Math.abs(+a || 0).toFixed(e))) + "",
    n = 3 < (n = g.length) ? n % 3 : 0;
  return (
    c +
    (n ? g.substr(0, n) + d : "") +
    g.substr(n).replace(/(\d{3})(?=\d)/g, "$1" + d) +
    (e
      ? b +
        Math.abs(a - g)
          .toFixed(e)
          .slice(2)
      : "")
  );
}

//Dictionary
const typestatus = ref([
  { value: 0, title: "Chưa hiệu lực", bg_color: "#bbbbbb", text_color: "#fff" },
  { value: 1, title: "Đang làm việc", bg_color: "#5fc57b", text_color: "#fff" },
  { value: 2, title: "Đã làm việc", bg_color: "red", text_color: "#fff" },
  { value: 3, title: "Đã làm việc", bg_color: "red", text_color: "#fff" },
]);

//Function
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};

//init
const initData = (rf) => {
  if (rf) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_myprofile_get_11",
            par: [
              { par: "profile_id", va: props.profile_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          insurance.value = tbs[0][0];
          if (insurance.value["status"] != null) {
            var idx = insurance_status.value.findIndex(
              (x) => x["status"] === insurance.value["status"]
            );
            if (idx != -1) {
              insurance.value["status_name"] = insurance_status.value["title"];
            }
          }
          if (insurance.value["insurance_province_id"] != null) {
            var idx = dictionarys.value.findIndex(
              (x) =>
                x["insurance_province_id"] ===
                insurance.value["insurance_province_id"]
            );
            if (idx != -1) {
              insurance.value["insurance_province_name"] =
                dictionarys.value[1]["insurance_province_name"];
            }
          }
        } else {
          insurance.value = {};
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          tbs[1].forEach((x) => {
            x["start_date"] = moment(new Date(x["start_date"])).format(
              "MM/YYYY"
            );
            x["total_payment"] = formatNumber(x["total_payment"], 0, ".", ".");
            x["company_payment"] = formatNumber(
              x["company_payment"],
              0,
              ".",
              "."
            );
            x["member_payment"] = formatNumber(
              x["member_payment"],
              0,
              ".",
              "."
            );
          });
          insurance_pays.value = tbs[1];
        } else {
          insurance_pays.value = [];
        }
        if (tbs[2] != null && tbs[2].length > 0) {
          tbs[2].forEach((x) => {
            x["received_file_date"] = moment(
              new Date(x["received_file_date"])
            ).format("DD/MM/YYYY");
            x["completed_date"] = moment(new Date(x["completed_date"])).format(
              "DD/MM/YYYY"
            );
            x["received_money_date"] = moment(
              new Date(x["received_money_date"])
            ).format("DD/MM/YYYY");
          });
          insurance_resolves.value = tbs[2];
        } else {
          insurance_resolves.value = [];
        }
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
onMounted(() => {
  initData(true);
});
</script>
<template>
  <div class="row">
    <div class="col-12 md:col-12 p-0">
      <Accordion Accordion class="w-full" :activeIndex="0">
        <AccordionTab>
          <template #header>
            <span>1. Thông tin chung</span>
          </template>
          <div class="col-12 md:col-12">
            <label
              >Số sổ bảo hiểm:
              <span class="description-2">{{
                insurance.insurance_id
              }}</span></label
            >
          </div>
          <div class="col-12 md:col-12">
            <label
              >Trạng thái:
              <span class="description-2">{{
                insurance.status_name
              }}</span></label
            >
          </div>
          <div class="col-12 md:col-12">
            <label
              >Pháp nhân đóng:
              <span class="description-2">{{
                insurance.organization_name
              }}</span></label
            >
          </div>
          <div class="col-12 md:col-12">
            <label
              >Số thẻ BHYT:
              <span class="description-2">{{
                insurance.insurance_code
              }}</span></label
            >
          </div>
          <div class="col-12 md:col-12">
            <label
              >Mã tỉnh cấp:
              <span class="description-2">{{
                insurance.insurance_province_name
              }}</span></label
            >
          </div>
          <div class="col-12 md:col-12">
            <label
              >Nơi đăng ký:
              <span class="description-2">{{
                insurance.hospital_name
              }}</span></label
            >
          </div>
        </AccordionTab>
      </Accordion>
      <Accordion Accordion class="w-full padding-0" :activeIndex="0">
        <AccordionTab>
          <template #header>
            <span>2. Lịch sử đóng bảo hiểm</span>
          </template>
          <div class="col-12 md:col-12 p-0">
            <DataTable
              :value="insurance_pays"
              :scrollable="true"
              :lazy="true"
              :rowHover="true"
              :showGridlines="true"
              scrollDirection="both"
              style="display: grid"
              class="empty-full"
            >
              <Column
                field="start_date"
                header="Từ tháng"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span>{{ slotProps.data.start_date }}</span>
                </template>
              </Column>
              <Column
                field="payment_form"
                header="Hình thức"
                headerStyle="text-align:center;width:170px;height:50px"
                bodyStyle="text-align:center;width:170px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <div class="form-group m-0">
                    <span>{{ slotProps.data.payment_form }}</span>
                  </div>
                </template>
              </Column>
              <Column
                field="reason"
                header="Lý do"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span>{{ slotProps.data.reason }}</span>
                </template>
              </Column>
              <Column
                field="organization_payment"
                header="Pháp nhân đóng"
                headerStyle="text-align:center;width:250px;height:50px"
                bodyStyle="text-align:center;width:250px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span>{{ slotProps.data.organization_payment }}</span>
                </template>
              </Column>
              <Column
                field="total_payment"
                header="Mức đóng"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span>{{ slotProps.data.total_payment }}</span>
                </template>
              </Column>
              <Column
                field="company_payment"
                header="Công ty đóng"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span>{{ slotProps.data.company_payment }}</span>
                </template>
              </Column>
              <Column
                field="member_payment"
                header="NLĐ đóng"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  {{ slotProps.data.member_payment }}
                </template>
              </Column>
              <template #empty>
                <div
                  class="align-items-center justify-content-center p-4 text-center m-auto"
                  style="display: flex; width: 100%"
                ></div>
              </template>
            </DataTable>
          </div>
        </AccordionTab>
      </Accordion>
      <Accordion Accordion class="w-full padding-0" :activeIndex="0">
        <AccordionTab>
          <template #header>
            <span>3. Lịch sử giải quyết chế độ</span>
          </template>
          <div class="col-12 md:col-12 p-0">
            <DataTable
              :value="insurance_resolves"
              :scrollable="true"
              :lazy="true"
              :rowHover="true"
              :showGridlines="true"
              style="display: grid"
              class="empty-full"
            >
              <Column
                field="type_mode"
                header="Loại chế độ"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span>{{ slotProps.data.type_mode }}</span>
                </template>
              </Column>
              <Column
                field="received_file_date"
                header="Ngày nhận hồ sơ"
                headerStyle="text-align:center;width:170px;height:50px"
                bodyStyle="text-align:center;width:170px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <div class="form-group m-0">
                    <span>{{ slotProps.data.received_file_date }}</span>
                  </div>
                </template>
              </Column>
              <Column
                field="completed_date"
                header="Ngày HT thủ tục"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span>{{ slotProps.data.completed_date }}</span>
                </template>
              </Column>
              <Column
                field="received_money_date"
                header="Ngày NT BH trả"
                headerStyle="text-align:center;width:250px;height:50px"
                bodyStyle="text-align:center;width:250px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span>{{ slotProps.data.received_money_date }}</span>
                </template>
              </Column>
              <Column
                field="money"
                header="Số tiền"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span>{{ slotProps.data.money }}</span>
                </template>
              </Column>
              <template #empty>
                <div
                  class="align-items-center justify-content-center p-4 text-center m-auto"
                  style="display: flex; width: 100%"
                ></div>
              </template>
            </DataTable>
          </div>
        </AccordionTab>
      </Accordion>
    </div>
  </div>
</template>
<style scoped>
.row {
  width: 100%;
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -ms-flex-wrap: wrap;
  flex-wrap: wrap;
}

.form-group {
  display: grid;
  margin-bottom: 1rem;
  flex: 1;
}

.form-group > label {
  margin-bottom: 0.5rem;
}

.ip36 {
  width: 100%;
}
.p-timeline-event-opposite {
  display: none;
}
.hover {
  cursor: pointer;
  color: #0078d4;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-timeline) {
  .p-timeline-event .p-timeline-event-opposite {
    display: none !important;
  }
  .p-timeline-event:nth-child(even) {
    flex-direction: row;
  }
}
</style>
