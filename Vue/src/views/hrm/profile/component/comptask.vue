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
  view: Number,
});

//Declare
const options = ref({});
const dictionarys = ref([]);
const task = ref({});
const tasks = ref([]);

//Dictionary
const typestatus = ref([
  { value: 0, title: "Chưa hiệu lực", bg_color: "#bbbbbb", text_color: "#fff" },
  { value: 1, title: "Đang làm việc", bg_color: "#2196f3", text_color: "#fff" },
  { value: 2, title: "Đã làm việc", bg_color: "red", text_color: "#fff" },
  { value: 3, title: "Đã làm việc", bg_color: "#ff8b4e", text_color: "#fff" },
]);

//init
const initDictionary2 = () => {
  dictionarys.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_dictionary",
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
          dictionarys.value = tbs;
        }
      }
    })
    .then(() => {
      initView2(true);
    });
};
const initView2 = (rf) => {
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
            proc: "hrm_profile_task_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
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
          task.value = tbs[0][0];
          var idx = typestatus.value.findIndex(
            (x) => x["value"] === task.value["status"]
          );
          if (idx != -1) {
            task.value["status_name"] = typestatus.value[idx]["title"];
            task.value["bg_color"] = typestatus.value[idx]["bg_color"];
            task.value["text_color"] = typestatus.value[idx]["text_color"];
          } else {
            task.value["status_name"] = "Chưa xác định";
            task.value["bg_color"] = "#bbbbbb";
            task.value["text_color"] = "#fff";
          }
          if (task.value["start_date"] != null) {
            task.value["start_date"] = moment(
              new Date(task.value["start_date"])
            ).format("DD/MM/YYYY");
          }
          if (task.value["end_date"] != null) {
            task.value["end_date"] = moment(
              new Date(task.value["end_date"])
            ).format("DD/MM/YYYY");
          }
          if (task.value["sign_date"] != null) {
            task.value["sign_date"] = moment(
              new Date(task.value["sign_date"])
            ).format("DD/MM/YYYY");
          }

          tbs[1].forEach((item) => {
            var idx = typestatus.value.findIndex(
              (x) => x["value"] === item["status"]
            );
            if (idx != -1) {
              item["status_name"] = typestatus.value[idx]["title"];
              item["bg_color"] = typestatus.value[idx]["bg_color"];
              item["text_color"] = typestatus.value[idx]["text_color"];
            } else {
              item["status_name"] = "Chưa xác định";
              item["bg_color"] = "#bbbbbb";
              item["text_color"] = "#fff";
            }
            if (item["start_date"] != null) {
              item["start_date"] = moment(new Date(item["start_date"])).format(
                "DD/MM/YYYY"
              );
            }
            if (item["end_date"] != null) {
              item["end_date"] = moment(new Date(item["end_date"])).format(
                "DD/MM/YYYY"
              );
            }
            if (item["sign_date"] != null) {
              item["sign_date"] = moment(new Date(item["sign_date"])).format(
                "DD/MM/YYYY"
              );
            }
          });
          tasks.value = tbs[1];
        } else {
          task.value = {};
          tasks.value = [];
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
  if (props.view === 2) {
    initView2(true);
  }
});
</script>
<template>
  <div
    class="d-lang-table my-3"
    :style="{
      height: 'calc(100vh - 182px) !important',
      overflowY: 'auto',
    }"
  >
    <Timeline :value="tasks" align="alternate" class="customized-timeline">
      <template #marker="slotProps">
        <span
          class="flex w-2rem h-2rem align-items-center justify-content-center text-white border-circle z-1 shadow-1"
          :style="{
            backgroundColor: slotProps.item.bg_color,
          }"
        >
          <i class="pi pi-briefcase"></i>
        </span>
      </template>
      <template #content="slotProps">
        <Card class="my-5">
          <template #title>
            <div class="w-full text-left">
              <Button
                :label="slotProps.item.status_name"
                class="p-button-outlined"
                :style="{
                  borderColor: slotProps.item.bg_color,
                  // backgroundColor: slotProps.data.bg_color,
                  color: slotProps.item.bg_color,
                  borderRadius: '15px',
                  padding: '0.3rem 0.75rem !important',
                }"
              />
            </div>
          </template>
          <template #subtitle>
            <div class="w-full text-left">
              {{ slotProps.item.sign_date }}
            </div>
          </template>
          <template #content>
            <div class="w-full text-left">
              <div class="mb-2">
                Chức danh: <b>{{ slotProps.item.work_position_name }}</b>
              </div>
              <div class="mb-2">
                Chức vụ: <b>{{ slotProps.item.position_name }}</b>
              </div>
              <div class="mb-2">
                Phòng ban: <b>{{ slotProps.item.department_name }}</b>
              </div>
              <div class="mb-2">
                Công việc chuyên môn:
                <b>{{ slotProps.item.professional_work_name }}</b>
              </div>
              <div>
                Loại hợp đồng: <b>{{ slotProps.item.type_contract_name }}</b>
              </div>
            </div>
          </template>
        </Card>
      </template>
    </Timeline>
  </div>

  <!-- <div class="row p-2">
    <div class="col-12 md:col-12 p-0">
      <Accordion class="w-full mb-2" :activeIndex="0">
        <AccordionTab>
          <template #header>
            <span>Công việc hiện tại</span>
          </template>
          <div class="row">
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label
                  >Trạng thái:
                  <span class="description-2">{{
                    task.status_name
                  }}</span></label
                >
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label
                  >Phòng ban:
                  <span class="description-2">{{
                    task.department_name
                  }}</span></label
                >
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label
                  >Vị trí:
                  <span class="description-2">{{
                    task.work_position_name
                  }}</span></label
                >
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label
                  >Chức vụ:
                  <span class="description-2">{{
                    task.position_name
                  }}</span></label
                >
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label
                  >Ngày hiệu lực:
                  <span class="description-2">{{ task.start_date }}</span>
                  <span v-if="task.start_date && task.end_date"> - </span>
                  <span v-if="task.end_date" class="description-2">{{
                    task.end_date
                  }}</span></label
                >
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label
                  >Ngày ký hợp đồng chính thức:
                  <span class="description-2">{{ task.sign_date }}</span></label
                >
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label
                  >Công việc chuyên môn:
                  <span class="description-2">{{
                    task.professional_work_name
                  }}</span></label
                >
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label
                  >Loại hợp đồng:
                  <span class="description-2">{{
                    task.contract_name
                  }}</span></label
                >
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label
                  >Hình thức:
                  <span class="description-2">{{
                    task.formality_name
                  }}</span></label
                >
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label
                  >Ngạch lương:
                  <span class="description-2">{{ task.wage_name }}</span></label
                >
              </div>
            </div>
          </div>
        </AccordionTab>
      </Accordion>
    </div>
    <div class="col-12 md:col-12 p-0">
      <Accordion class="w-full padding-0 mb-2" :activeIndex="0">
        <AccordionTab>
          <template #header>
            <span>Quá trình làm việc</span>
          </template>
          <div>
            <DataTable
              :value="tasks"
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
                header="Ngày hiệu lực"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span v-html="slotProps.data.start_date"></span>
                </template>
              </Column>
              <Column
                field="start_date"
                header="Ngày hết hạn"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span v-html="slotProps.data.end_date"></span>
                </template>
              </Column>

              <Column
                field="department_name"
                header="Phòng ban"
                headerStyle="text-align:center;width:250px;height:50px"
                bodyStyle="text-align:center;width:250px;"
                class="align-items-center justify-content-center text-center"
              />
              <Column
                field="work_position_name"
                header="Vị trí"
                headerStyle="text-align:center;width:200px;height:50px"
                bodyStyle="text-align:center;width:200px;"
                class="align-items-center justify-content-center text-center"
              />
              <Column
                field="position_name"
                header="Chức vụ"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              />
              <Column
                field="type_contract_name"
                header="Loại hợp đồng"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  {{ slotProps.data.type_contract_name }}
                </template>
              </Column>
              <Column
                field="contract_no"
                header="Mã HĐ"
                headerStyle="text-align:center;width:80px;height:50px"
                bodyStyle="text-align:center;width:80px;"
                class="align-items-center justify-content-center text-center"
              />
              <Column
                field="status"
                header="Trạng thái"
                headerStyle="text-align:center;width:140px;height:50px"
                bodyStyle="text-align:center;width:140px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <div
                    class="m-2"
                    aria:haspopup="true"
                    aria-controls="overlay_panel_status"
                  >
                    <Button
                      :label="slotProps.data.status_name"
                      :style="{
                        border: slotProps.data.bg_color,
                        backgroundColor: slotProps.data.bg_color,
                        color: slotProps.data.text_color,
                      }"
                    />
                  </div>
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
  </div> -->
</template>
<style scoped></style>
