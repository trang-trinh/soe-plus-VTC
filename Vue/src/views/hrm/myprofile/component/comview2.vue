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
const skills = ref([]);

//Function
function urlify(string) {
  var urlRegex = string.match(
    /((((ftp|https?):\/\/)|(w{3}\.))[\-\w@:%_\+.~#?,&\/\/=]+)/g
  );
  if (urlRegex) {
    urlRegex.forEach(function (url) {
      string = string.replace(
        url,
        '<a target="_blank" href="' + url + '">' + url + "</a>"
      );
    });
  }
  return string.replace("(", "<br/>(");
}
const trustAsHtml = (html) => {
  if (!html) {
    return "";
  }
  return urlify(html).replace(/\n/g, "<br/>");
};

//init
const initView2 = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  profile.value = {};
  skills.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_myprofile_get_2",
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

            if (profile.value.salary_family != null) {
              profile.value.salary_family = parseFloat(
                profile.value.salary_family
              ).toLocaleString("vi-vN", {
                style: "decimal",
                minimumFractionDigits: 0,
                maximumFractionDigits: 20,
              });
            }
            if (profile.value.salary_orther != null) {
              profile.value.salary_orther = parseFloat(
                profile.value.salary_orther
              ).toLocaleString("vi-vN", {
                style: "decimal",
                minimumFractionDigits: 0,
                maximumFractionDigits: 20,
              });
            }

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
          if (data[4] != null && data[4].length > 0) {
            skills.value = data[4];
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
  nextTick(() => {
    initView2(true);
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
              <span
                >Quá trình đào tạo, bồi dưỡng về chuyên môn, nghiệp vụ, lý luận
                chính trị, ngoại ngữ, tin học</span
              >
            </div>
          </div>
          <Timeline
            :value="skills"
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
                <i class="pi pi-book"></i>
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
                        Nơi dào tạo:
                        <span>{{ slotProps.item.university_name }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Chuyên ngành:
                        <span>{{ slotProps.item.specialization_name }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Hệ đào tạo:
                        <span>{{ slotProps.item.form_traning_name }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Xếp loại:
                        <span>{{ slotProps.item.rating }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Năm tốt nghiệp:
                        <span>{{ slotProps.item.graduation }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Ngày cấp bằng:
                        <span>{{ slotProps.item.degree_date_string }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Trình độ chuyên môn:
                        <span>{{ slotProps.item.academic_level_name }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Ngày hiệu lực:
                        <span>{{
                          slotProps.item.certificate_start_date_string
                        }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Ngày hết hiệu lực:
                        <span>{{
                          slotProps.item.certificate_end_date_string
                        }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Loại văn bằng:
                        <span>{{ slotProps.item.academic_level_name }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Số hiệu:
                        <span>{{ slotProps.item.certificate_key_code }}</span>
                      </label>
                    </div>
                  </div>
                </template>
              </Card>
            </template>
          </Timeline>
        </div>
      </div>
    </div>
    <div class="col-8 md:col-8 p-0">
      <div class="row">
        <div class="col-6 md:col-6">
          <div class="my-2">
            <div class="card mt-2">
              <div class="card-header">
                <span>Thông tin tham gia quân đội</span>
              </div>

              <div class="card-body">
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label class="m-0">
                      <span>Ngày nhập ngũ: </span>
                      <span>{{ profile.military_start_date_string }}</span>
                    </label>
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label class="m-0">
                      <span>Ngày xuất ngũ: </span>
                      <span>{{ profile.military_end_date_string }}</span>
                    </label>
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label class="m-0">
                      <span>Quân hàm cao nhất: </span>
                      <span>{{ profile.military_rank }}</span>
                    </label>
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label class="m-0">
                      <span>Danh hiệu phong tặng cao nhất: </span>
                      <span>{{ profile.military_title }}</span>
                    </label>
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label class="m-0">
                      <span>Thương binh hạng: </span>
                      <span>{{ profile.military_veterans_rank }}</span>
                    </label>
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group m-0">
                    <label class="m-0">
                      <span>Con gia đình chính sách: </span>
                      <span>{{ profile.military_policy_family }}</span>
                    </label>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="my-2">
            <div class="card mt-2">
              <div class="card-header">
                <span>Đặc điểm lịch sử bản thân</span>
              </div>

              <div class="card-body">
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label class="m-0">
                      <span>Thành phần gia dình xuất thân: </span>
                      <span>{{ profile.is_partisan }}</span>
                    </label>
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label class="m-0">
                      <span
                        >Nghề nghiệp bản thân trước khi được tuyển dụng:
                      </span>
                      <span>{{ profile.job_before_recruitment }}</span>
                    </label>
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label class="m-0">
                      <span>Công việc đã làm lâu nhất: </span>
                      <span>{{ profile.task_longest }}</span>
                    </label>
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label class="m-0">
                      <span>Sở trường công tác: </span>
                      <span>{{ profile.mission_forte }}</span>
                    </label>
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label class="m-0">
                      <span>Khen thưởng: <br /></span>
                      <span
                        v-html="trustAsHtml(profile.military_reward)"
                      ></span>
                    </label>
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group m-0">
                    <label class="m-0">
                      <span>Kỷ luật: <br /></span>
                      <span
                        v-html="trustAsHtml(profile.military_discipline)"
                      ></span>
                    </label>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="my-2">
            <div class="card mt-2">
              <div class="card-header">
                <span>Nguồn thu nhập chính của gia đình</span>
              </div>
              <div class="card-body">
                <div class="row">
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label class="m-0"
                        >Lương gia đình:
                        <span>{{ profile.salary_family }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0"
                        >Nguồn khác:
                        <span>{{ profile.salary_orther }}</span></label
                      >
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Được cấp, được thuê, loại nhà:
                        <span>{{ profile.type_rent }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Diện tích nhà sử dụng:
                        <span>{{ profile.area_level }} m2</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0"
                        >Nhà tự mua, loại nhà:
                        <span>{{ profile.type_house }}</span></label
                      >
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label class="m-0"
                        >Diện tích nhà mua:
                        <span>{{ profile.area_buy }} m2</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0"
                        >Diện tích dất được cấp:
                        <span>{{ profile.area_granted }} m2</span></label
                      >
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Diện tích dất tự mua:
                        <span>{{ profile.area_buy_yourself }} m2</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0">
                        Tổng diện tích:
                        <span>{{ profile.area_manufacture }}</span>
                      </label>
                    </div>
                    <div class="form-group">
                      <label class="m-0"
                        >Địa điểm ký:
                        <span>{{ profile.sign_address }}</span></label
                      >
                    </div>
                    <div class="form-group">
                      <label class="m-0"
                        >Ngày ký:
                        <span>{{ profile.sign_date_string }}</span></label
                      >
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
