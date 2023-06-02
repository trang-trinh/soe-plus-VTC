<script setup>
import { onMounted, ref, inject, nextTick } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";

const router = inject("router");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const base_url = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  profile_id: String,
});

//Declare
const options = ref({
  loading: true,
});
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const typestatus = ref([
  { value: 0, title: "Đang làm việc", bg_color: "#5fc57b", text_color: "#fff" },
  { value: 1, title: "Đã làm việc", bg_color: "red", text_color: "#fff" },
]);

const profile = ref({});
const in_tasks = ref([]);
const out_tasks = ref([]);
const relatives = ref([]);

//init
const initView1 = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  profile.value = {};
  in_tasks.value = [];
  out_tasks.value = [];
  relatives.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_myprofile_get_1",
            par: [{ par: "profile_id", va: props.profile_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        let data = JSON.parse(response.data.data);
        if (data != null) {
          if (data[0] != null && data[0].length > 0) {
            profile.value = data[0][0];

            const startDate = moment(
              profile.value.recruitment_date || new Date()
            );
            const endDate = moment(new Date());
            profile.value.duration = moment.duration(endDate.diff(startDate));
            profile.value.diffyear = profile.value.duration.years();
            profile.value.diffmonth = profile.value.duration.months();
            profile.value.diffday = profile.value.duration.days();
          } else {
            profile.value = {};
            swal.fire({
              title: "Thông báo!",
              text: "Tài khoản truy cập cùa bạn chưa liên kết đến hồ sơ!",
              icon: "error",
              confirmButtonText: "OK",
            });
            return;
          }
          if (data[1] != null && data[1].length > 0) {
            data[1].forEach((item) => {
              if (item.is_active) {
                item["status_name"] = typestatus.value[0]["title"];
                item["bg_color"] = typestatus.value[0]["bg_color"];
                item["text_color"] = typestatus.value[0]["text_color"];
              } else {
                item["status_name"] = typestatus.value[1]["title"];
                item["bg_color"] = typestatus.value[1]["bg_color"];
                item["text_color"] = typestatus.value[1]["text_color"];
              }
            });
            in_tasks.value = data[1];
          }
          if (data[2] != null && data[2].length > 0) {
            data[2].forEach((item) => {
              if (item.wage != null) {
                item.wage_string = parseFloat(item.wage).toLocaleString(
                  "vi-vN",
                  {
                    style: "decimal",
                    minimumFractionDigits: 0,
                    maximumFractionDigits: 20,
                  }
                );
              }
            });
            out_tasks.value = data[2];
          }
          if (data[3] != null && data[3].length > 0) {
            relatives.value = data[3];
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
  nextTick(() => {
    initView1(true);
  });
});
</script>
<template>
  <div class="box-info grid formgrid m-0">
    <div class="col-4 md:col-4">
      <div class="row">
        <div class="col-12 md:col-12 p-0">
          <div class="card mt-2">
            <div class="card-header">
              <span>Thông tin liên hệ</span>
            </div>
            <div class="card-body">
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <i class="pi pi-mobile mr-3"></i>
                    <span>Mobile: </span>
                    <b>{{ profile.phone }}</b>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <i class="pi pi-phone mr-3"></i>
                    <span>Điện thoại cố định: </span>
                    <b>{{ profile.fax }}</b>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <i class="pi pi-envelope mr-3"></i>
                    <span>Email: </span>
                    <b>{{ profile.email }}</b>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <i class="pi pi-calendar mr-3"></i>
                    <span>Ngày sinh: </span>
                    <b>{{ profile.birthday }}</b>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <i class="pi pi-calendar mr-3"></i>
                    <span>Giới tinh: </span>
                    <b>{{ profile.gender }}</b>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <i class="pi pi-heart mr-3"></i>
                    <span>Tình trạng hôn nhân: </span>
                    <b>{{ profile.profile_relate }}</b>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <i class="pi pi-key mr-3"></i>
                    <span>Tài khoản truy cập: </span>
                    <b>{{ profile.user_id }}</b>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group m-0">
                  <label class="m-0">
                    <i class="pi pi-home mr-3"></i>
                    <span>Nơi ở hiện nay: </span>
                    <b>{{ profile.place_residence_name }}</b>
                  </label>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="card mt-3">
            <div class="card-header">
              <span>Thông tin công việc</span>
            </div>
            <div class="card-body">
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Chức vụ: </span>
                    <span>{{ profile.position_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Chức danh: </span>
                    <span>{{ profile.title_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Đơn vị/Phòng ban: </span>
                    <span>{{ profile.department_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Công việc chuyên môn: </span>
                    <span>{{ profile.professional_work_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Loại nhân sự: </span>
                    <span>{{ profile.personel_groups_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Cấp nhân sự: </span>
                    <span>{{ profile.personnel_level_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Nghạch công chức/viên chức: </span>
                    <span>{{ profile.civil_servant_rank_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group m-0">
                  <label class="m-0">
                    <span>Người quản lý trực tiếp: </span>
                    <span>{{ profile.manager_name }}</span>
                  </label>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="card mt-3">
            <div class="card-header">
              <span>Thông tin chung</span>
            </div>
            <div class="card-body">
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Số CMND/CCCD: </span>
                    <span>{{ profile.identity_papers_code }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Ngày cấp: </span>
                    <span>{{ profile.identity_date_issue }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Nơi cấp: </span>
                    <span>{{ profile.identity_place_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Ngày hết hạn: </span>
                    <span>{{ profile.identity_end_date_issue }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Mã số thuế: </span>
                    <span>{{ profile.tax_code }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Nơi sinh: </span>
                    <span>{{ profile.birthplace_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Quê quán: </span>
                    <span>{{ profile.birthplace_origin_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Quốc tịch: </span>
                    <span>{{ profile.nationality_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Dân tộc: </span>
                    <span>{{ profile.ethnic_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Tôn giáo: </span>
                    <span>{{ profile.religion_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group m-0">
                  <label class="m-0">
                    <span>Thường trú: </span>
                    <span>{{ profile.place_residence_name }}</span>
                  </label>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="card mt-3">
            <div class="card-header">
              <span>Thông tin về trình độ</span>
            </div>
            <div class="card-body">
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Trình độ phổ thông: </span>
                    <span>{{ profile.cultural_level_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Trình độ học vấn cao nhất: </span>
                    <span>{{ profile.academic_level_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Chuyên ngành học: </span>
                    <span>{{ profile.specialization_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Hình thức đào tạo: </span>
                    <span>{{ profile.form_traning_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Ngày tốt nghiệp: </span>
                    <span>{{ profile.graduation_year }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Ngoại ngữ: </span>
                    <span>{{ profile.language_level_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Quản lý nhà nước: </span>
                    <span>{{ profile.management_state_name }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group m-0">
                  <label class="m-0">
                    <span>Lý luận chính trị: </span>
                    <span>{{ profile.political_theory_name }}</span>
                  </label>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="card mt-3">
            <div class="card-header">
              <span>Thông tin về tham gia Đảng/tổ chức chính trị</span>
            </div>
            <div class="card-body">
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Là Đảng viên: </span>
                    <span>{{ profile.is_partisan }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Số thẻ Đảng: </span>
                    <span>{{ profile.card_partisan }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Ngày vào Đảng: </span>
                    <span>{{ profile.partisan_date }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Ngày vào Đảng chính thức: </span>
                    <span>{{ profile.partisan_main_date }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Chị bộ sinh hoạt Đảng: </span>
                    <span>{{ profile.partisan_branch }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group m-0">
                  <label class="m-0">
                    <span>Đảng bộ chính thức: </span>
                    <span>{{ profile.partisan_official }}</span>
                  </label>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="card mt-3">
            <div class="card-header">
              <span>Thông tin về sức khỏe</span>
            </div>
            <div class="card-body">
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Tình trạng: </span>
                    <span>{{ profile.military_health }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Chiểu cao: </span>
                    <span>{{ profile.height }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Cân nặng: </span>
                    <span>{{ profile.weight }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Nhóm máu: </span>
                    <span>{{ profile.blood_group }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Huyết áp: </span>
                    <span>{{ profile.blood_pressure }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label class="m-0">
                    <span>Nhịp tim: </span>
                    <span>{{ profile.heartbeat }}</span>
                  </label>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group m-0">
                  <label class="m-0">
                    <span>Số mũi tiêm phòng covid: </span>
                    <span>{{ profile.total_vaccine }}</span>
                  </label>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="col-8 md:col-8 p-0">
      <div class="row">
        <div class="col-6 md:col-6">
          <div class="my-2">
            <div class="card mt-2">
              <div class="card-header">
                <span>Quá trình công tác trong đơn vị</span>
              </div>
            </div>
            <Timeline
              :value="in_tasks"
              align="alternate"
              class="customized-timeline"
            >
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
                <Card :style="{ border: 'none', boxShadow: 'none' }">
                  <template #subtitle>
                    <div class="w-full text-left">
                      <Button
                        :label="slotProps.item.status_name"
                        icon="pi pi-chevron-down"
                        iconPos="right"
                        class="p-button-outlined"
                        :style="{
                          borderColor: slotProps.item.bg_color,
                          // backgroundColor: slotProps.data.bg_color,
                          color: slotProps.item.bg_color,
                          borderRadius: '15px',
                          padding: '0.3rem 0.75rem !important',
                          minWidth: 'max-content',
                        }"
                      >
                        <div>
                          Từ: <b>{{ slotProps.item.start_date_string }}</b>
                          <span v-if="slotProps.item.end_date_string">
                            Đến: <b>{{ slotProps.item.end_date_string }}</b>
                          </span>
                        </div>
                      </Button>
                    </div>
                  </template>
                  <template #content>
                    <div class="w-full text-left">
                      <div class="form-group">
                        <label class="m-0">
                          Chức vụ:
                          <span>{{ slotProps.item.position_name }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Chức danh:
                          <span>{{ slotProps.item.title_name }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Đơn vị:
                          <span>{{ slotProps.item.organization_name }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Phong ban:
                          <span>{{ slotProps.item.department_name }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Công việc chuyên môn:
                          <span>{{
                            slotProps.item.professional_work_name
                          }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Nơi làm việc:
                          <span>{{ slotProps.item.employment }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Hình thức làm việc:
                          <span>{{ slotProps.item.formality_name }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Loại nhân sự:
                          <span>{{ slotProps.item.personel_groups_name }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Cấp nhân sự:
                          <span>{{ slotProps.item.personnel_level_name }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Người quản lý:
                          <span>{{ slotProps.item.manager_name }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Vai trò:
                          <span>{{ slotProps.item.formality_name }}</span>
                        </label>
                      </div>
                    </div>
                  </template>
                </Card>
              </template>
            </Timeline>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="my-2">
            <div class="card mt-2">
              <div class="card-header">
                <span>Quá trình công tác(đơn vị cũ)/Kinh nghiệm làm việc</span>
              </div>
            </div>
            <Timeline
              :value="out_tasks"
              align="alternate"
              class="customized-timeline"
            >
              <template #marker="slotProps">
                <span
                  class="flex w-2rem h-2rem align-items-center justify-content-center text-white border-circle z-1 shadow-1"
                  :style="{
                    backgroundColor: '#6c757d',
                  }"
                >
                  <i class="pi pi-briefcase"></i>
                </span>
              </template>
              <template #content="slotProps">
                <Card :style="{ border: 'none', boxShadow: 'none' }">
                  <template #subtitle>
                    <div class="w-full text-left">
                      <Button
                        :label="slotProps.item.status_name"
                        icon="pi pi-chevron-down"
                        iconPos="right"
                        class="p-button-outlined"
                        :style="{
                          borderColor: '#6c757d',
                          // backgroundColor: slotProps.data.bg_color,
                          color: '#6c757d',
                          borderRadius: '15px',
                          padding: '0.3rem 0.75rem !important',
                          minWidth: 'max-content',
                        }"
                      >
                        <div>
                          Từ: <b>{{ slotProps.item.start_date_string }}</b>
                          <span v-if="slotProps.item.end_date_string">
                            Đến: <b>{{ slotProps.item.end_date_string }}</b>
                          </span>
                        </div>
                      </Button>
                    </div>
                  </template>
                  <template #content>
                    <div class="w-full text-left">
                      <div class="form-group">
                        <label class="m-0">
                          Đơn vị:
                          <span>{{ slotProps.item.company }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Địa chỉ:
                          <span>{{ slotProps.item.address }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Vị trí:
                          <span>{{ slotProps.item.role }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Mức lương:
                          <span v-if="slotProps.item.wage_string"
                            >{{ slotProps.item.wage_string }} đ</span
                          >
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Người tham chiếu:
                          <span>{{ slotProps.item.reference_name }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Mô tả công việc:
                          <span>{{ slotProps.item.description }}</span>
                        </label>
                      </div>
                      <div class="form-group">
                        <label class="m-0">
                          Lý do nghỉ việc:
                          <span>{{ slotProps.item.reason }}</span>
                        </label>
                      </div>
                    </div>
                  </template>
                </Card>
              </template>
            </Timeline>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="my-2">
            <div class="card mt-2">
              <div class="card-header">
                <span>Thông tin gia đình, người phụ thuộc</span>
              </div>
              <div class="card-body">
                <div class="row">
                  <div
                    class="col-6 md:col-6"
                    :class="{ 'mb-3': index != relatives.length - 1 }"
                    v-for="(item, index) in relatives"
                  >
                    <div class="form-group">
                      <label class="m-0">
                        {{ index + 1 }}. Họ và tên:
                        <b>{{ item.relative_name }}</b>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0"
                        >Quan hệ:
                        <span
                          >{{ item.relationship_name }} ({{
                            item.type_string
                          }})</span
                        ></label
                      >
                    </div>
                    <div class="form-group">
                      <label class="m-0"
                        >Năm sinh:
                        <span>{{ item.birthday_string }}</span></label
                      >
                    </div>
                    <div class="form-group">
                      <label class="m-0"
                        >Số điện thoại: <span>{{ item.phone }}</span></label
                      >
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        CCCD/Hộ chiếu:
                        <span>{{ item.identification_citizen }}</span>
                        <span
                          v-if="
                            item.identification_citizen &&
                            item.identification_date_issue_string
                          "
                          >|</span
                        >
                        <span>{{ item.identification_date_issue_string }}</span>
                        <span
                          v-if="
                            item.identification_date_issue_string &&
                            item.identification_place_issue
                          "
                          >|</span
                        >
                        <span>{{ item.identification_place_issue }}</span>
                      </label>
                    </div>
                    <div v-if="item.is_company" class="form-group">
                      <label class="m-0">Cùng cơ quan <span></span></label>
                    </div>
                    <div v-if="item.is_die" class="form-group">
                      <label class="m-0">Đã mất <span></span></label>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
@import url(../../contract/component/stylehrm.css);
.box-info .card {
  border: none;
  border-radius: 0;
  position: relative;
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -webkit-box-orient: vertical;
  -webkit-box-direction: normal;
  -ms-flex-direction: column;
  flex-direction: column;
  min-width: 0;
  word-wrap: break-word;
  background-color: #fff;
  background-clip: border-box;
}
.box-info .card-header {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
  border-bottom: solid 1px rgba(0, 0, 0, 0.1);
  font-size: 15px;
  font-weight: bold;
  color: #005a9e;
}
.box-info .card-body {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
}
</style>
<style lang="scss" scoped>
::v-deep(.border-radius) {
  img {
    border-radius: 50%;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-timeline) {
  background-color: #fff;
  padding: 1rem;
  .p-timeline-event .p-timeline-event-opposite {
    display: none !important;
  }
  .p-timeline-event:nth-child(even) {
    flex-direction: row;
  }
  .p-card-body {
    padding: 0;
  }
}
</style>
