<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const router = inject("router");

const axios = inject("axios");
const store = inject("store");
const basedomainURL = fileURL;
let id = store.state.user.user_id;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

const currentDay = new Date();
const list_birthday_comming = ref([]);
const list_birthday_recently = ref([]);
const list_birthday_today = ref([]);
const list_birthday_tomonthother = ref([]);
const toDay = ref({});
const ListThang = ref([]);
const sinhnhat = ref([]);
const offset_top = ref("");
const GetNhanSu_ID = ref("");
const heightSN = ref();
toDay.value = {
  Ngay: currentDay.getDate(),
  Thu: currentDay.getDay() !== 7 ? currentDay.getDay() + 1 : "CN",
  Nam: currentDay.getFullYear(),
  Nam2: currentDay.getYear(),
  Gio: currentDay.getHours(),
  Tt: currentDay.getMilliseconds(),
  Phut: currentDay.getMinutes(),
  Thang: currentDay.getMonth() + 1,
  Giay: currentDay.getSeconds(),
  Time: currentDay.getTime(),
};
const dateAgo = (ngaysinhnhat) => {
  var ngay = new Date(ngaysinhnhat);
  ngay = new Date(currentDay.getFullYear(), ngay.getMonth(), ngay.getDate());
  if (ngay) {
    if (
      ngay.getDate() === toDay.value.Ngay - 1 &&
      ngay.getMonth() === toDay.value.Thang + 1
    ) {
      return "Hôm qua";
    }
    return ngay.getDay() !== 7 && ngay.getDay() > 0
      ? "Thứ " + (ngay.getDay() + 1)
      : "Chủ nhật";
  }
};
const Get_BirthdayUserRecently = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "birthday_getbirdthday_recently",
            par: [
              { par: "myDate", va: currentDay },
              { par: "user", va: id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        let obj = groupBy(data[0], "ngaySinhNhat");
        var result = Object.entries(obj);
        result.forEach((item) => {
          let obj = {
            ngaySinhNhat: item[0],
            day: dateAgo(item[0]),
            users: item[1],
          };
          list_birthday_recently.value.push(obj);
        });
        // list_birthday_recently.value = data[0];
        // data[0].forEach((item) => {
        //   let obj= {};
        //   obj.ngaySinhNhat= Object.keys(item);
        //   list_birthday_recently.value.push(obj);
        // })
      }
    });
};
const Get_BirthdayUserComming = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "birthday_getbirdthday_comming",
            par: [
              { par: "myDate", va: currentDay },
              { par: "user", va: id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        list_birthday_comming.value = data[0];
        list_birthday_comming.value.forEach((item) => {
          item.birthDate = dateAgo(item.birthday);
        });
      }
    });
};
const Get_BirthdayUserToday = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "birthday_getbirdthday_today",
            par: [
              { par: "myDate", va: currentDay },
              { par: "user", va: id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        list_birthday_today.value = data[0];
      }
    });
};
const GetListMonth = () => {
  for (var i = 1; i < 13; i++) {
    if (i >= toDay.value.Thang) {
      ListThang.value.push({ thang: i });
    }
  }
  for (var j = 1; j < 13; j++) {
    if (j < toDay.value.Thang) {
      ListThang.value.push({ thang: j });
    }
  }
};
const Get_BirthdayUserToMonthOther = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "birthday_getbirdthday_tomonthother",
            par: [
              { par: "myDate", va: currentDay },
              { par: "user", va: id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        list_birthday_tomonthother.value = data[0];
        ListThang.value.forEach((m) => {
          let obj = { thang: m.thang, countUsers: 0, users: [] };
          let arrUser = list_birthday_tomonthother.value.filter(
            (x) => x.miy == m.thang,
          );
          if (arrUser.length > 0) obj.users.push(arrUser);
          obj.countUsers = arrUser.length;
          sinhnhat.value.push(obj);
        });
        heightSN.value = document.getElementById("SinhNhatGanDay").offsetHeight;
      }
    });
};
// mouseleave,mouseover
const ShowInForUser = (m) => {
  var id = m.user_id.replace(".", "");
  var item = document.getElementById(id);
  offset_top.value = item.getBoundingClientRect().top - 80 + "px !important";
  if (GetNhanSu_ID.value == m.user_id) {
    GetNhanSu_ID.value = m.user_id;
    return false;
  }
  GetNhanSu_ID.value = m.user_id;
};
const HidenInForUser = () => {
  GetNhanSu_ID.value = "";
};
var groupBy = function (xs, key) {
  if (xs != null)
    if (xs.length > 0)
      return xs.reduce(function (rv, x) {
        (rv[x[key]] = rv[x[key]] || []).push(x);
        return rv;
      }, {});
};
const ChucMung = (m) => {
  store.commit("setbirthDay_id", m);
  if (router)
    router.push({
      path: "/birthday/send",
    });
};
onMounted(() => {
  bgColor;
  Get_BirthdayUserRecently();
  Get_BirthdayUserComming();
  Get_BirthdayUserToday();
  GetListMonth();
  Get_BirthdayUserToMonthOther();
  return {};
});
</script>
<template>
  <div class="surface-100">
    <div class="m-4">
      <div
        class="scroll-outer"
        style="height: calc(100vh - 40px); overflow-x: hidden; overflow-y: auto"
      >
        <div class="scroll-inner">
          <div class="container-fluid p-0">
            <div class="row">
              <div class="col-md-9 p-0">
                <div class="m-1">
                  <div class="card-body cardfollow p-0">
                    <div class="table-stats">
                      <!-- <div class="scroll-outer" style="min-height: calc(100vh - 130px);max-height: calc(100vh - 130px);overflow: hidden;"> -->
                      <div class="scroll-outer">
                        <div class="scroll-inner">
                          <!-- Center Side -->
                          <div
                            class="row center-side m-0 p-0"
                            style="overflow: hidden; width: 100%"
                          >
                            <div
                              class="col-md-12"
                              style="width: 100%"
                            >
                              <div class="row">
                                <div
                                  class="scroll-outer"
                                  style="
                                    width: 100%;
                                    padding: 0;
                                    overflow-x: hidden;
                                    overflow-y: auto;
                                    min-height: calc(100vh - 90px);
                                    max-height: calc(100vh - 90px);
                                  "
                                >
                                  <div class="scroll-inner">
                                    <div
                                      class="col-md-12 box-homnay"
                                      v-if="list_birthday_today.length > 0"
                                    >
                                      <div class="row row-box-homnay">
                                        <div class="col-md-12 box-sn-hn">
                                          <span>
                                            Sinh nhật vào hôm nay ({{
                                              toDay.Ngay +
                                              "/" +
                                              toDay.Thang +
                                              "/" +
                                              toDay.Nam
                                            }})
                                          </span>
                                        </div>
                                        <div class="col-md-12">
                                          <div
                                            class="box-item-snhn"
                                            v-for="(
                                              m, index
                                            ) in list_birthday_today"
                                            :key="index"
                                          >
                                            <div
                                              class="img-user-sn"
                                              data-toggle="tooltip"
                                              data-placement="top"
                                              tooltip-placement="auto"
                                              uib-tooltip-html="m.full_name+' ('+(m.birthday | date:'dd/MM')+')'+'<br/>'+m.tenChucVu+'<br/>'+m.tenToChuc"
                                            >
                                              <Avatar
                                                v-bind:label="
                                                  m.avatar
                                                    ? ''
                                                    : m.last_name.substring(
                                                        0,
                                                        1,
                                                      )
                                                "
                                                v-bind:image="
                                                  basedomainURL + m.avatar
                                                "
                                                style="
                                                  background-color: #2196f3;
                                                  color: #ffffff;
                                                "
                                                :style="{
                                                  background:
                                                    bgColor[index % 7],
                                                }"
                                                class="mr-2"
                                                size="xlarge"
                                                shape="circle"
                                              />
                                            </div>
                                            <div class="info-user-sn">
                                              <span class="info-user-sn-name">{{
                                                m.full_name
                                              }}</span
                                              ><br />
                                              <span class="info-user-sn-tuoi"
                                                >Bước sang tuổi
                                                {{ m.tuoi }}</span
                                              ><br />

                                              <span
                                                @click="ChucMung(m)"
                                                v-if="
                                                  store.state.user.user_id !=
                                                  m.user_id
                                                "
                                                class="btn-gui-loi-chuc"
                                                >Gửi lời chúc
                                              </span>
                                              <span
                                                v-if="
                                                  store.state.user.user_id ==
                                                  m.user_id
                                                "
                                                @click="ChucMung(m)"
                                                class="btn-gui-loi-chuc"
                                              >
                                                Xem lời chúc
                                              </span>
                                            </div>
                                          </div>
                                        </div>
                                      </div>
                                    </div>
                                    <div class="col-md-12 box-saptoi">
                                      <div class="row row-box-saptoi">
                                        <div class="col-md-12 box-sn-hn">
                                          <span> Sinh nhật sắp tới </span>
                                        </div>
                                        <div class="col-md-12">
                                          <div
                                            class="box-item-snhn"
                                            v-for="(
                                              m, index
                                            ) in list_birthday_comming"
                                            :key="index"
                                          >
                                            <div
                                              class="img-user-sn"
                                              data-toggle="tooltip"
                                            >
                                              <Avatar
                                                v-bind:label="
                                                  m.avatar
                                                    ? ''
                                                    : m.last_name.substring(
                                                        0,
                                                        1,
                                                      )
                                                "
                                                v-bind:image="
                                                  basedomainURL + m.avatar
                                                "
                                                style="
                                                  background-color: #2196f3;
                                                  color: #ffffff;
                                                "
                                                :style="{
                                                  background:
                                                    bgColor[index % 7],
                                                }"
                                                class="mr-2"
                                                size="xlarge"
                                                shape="circle"
                                              />
                                            </div>
                                            <div class="info-user-sn">
                                              <span class="info-user-sn-name">{{
                                                m.full_name
                                              }}</span
                                              ><br />
                                              <span class="info-user-sn-tuoi"
                                                >Bước sang tuổi
                                                {{ m.tuoi }}</span
                                              ><br />
                                              <span style="font-size: 12px"
                                                >{{ m.birthDate }} ({{
                                                  moment(
                                                    new Date(m.birthday),
                                                  ).format("DD/MM")
                                                }})</span
                                              >
                                            </div>
                                          </div>
                                        </div>
                                      </div>
                                      <div
                                        class="row row-thang"
                                        v-for="(m, index1) in sinhnhat"
                                        :key="index1"
                                      >
                                        <div class="col-md-12 item-thang">
                                          <span>Tháng {{ m.thang }}</span>
                                          <span
                                            class="count-ns-sn"
                                            v-tooltip.top="'Tổng số'"
                                            >{{ m.countUsers }} người</span
                                          >
                                        </div>
                                        <div class="col-md-12">
                                          <div
                                            class="img-user-sn"
                                            v-for="(u, index) in m.users[0]"
                                            :key="index"
                                          >
                                            <div v-if="u.display_birthday">
                                              <Avatar
                                                v-tooltip.right="{
                                                  value:
                                                    u.full_name +
                                                    ' (' +
                                                    u.ngay +
                                                    '/' +
                                                    u.miy +
                                                    ') ' +
                                                    '<br/>' +
                                                    (u.tenChucVu || '') +
                                                    '<br/>' +
                                                    (u.tenToChuc || ''),
                                                  escape: true,
                                                }"
                                                v-bind:label="
                                                  u.avatar
                                                    ? ''
                                                    : u.last_name.substring(
                                                        0,
                                                        1,
                                                      )
                                                "
                                                v-bind:image="
                                                  basedomainURL + u.avatar
                                                "
                                                style="
                                                  background-color: #2196f3;
                                                  color: #ffffff;
                                                  cursor: pointer;
                                                "
                                                :style="{
                                                  background:
                                                    bgColor[index % 7],
                                                }"
                                                class="mr-2"
                                                size="xlarge"
                                                shape="circle"
                                              />
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
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="col-md-3">
                <div class="row">
                  <div
                    class="col-md-12"
                    style="padding: 0 7px"
                  >
                    <!-- Right Side -->
                    <div
                      class="right-side"
                      id="SinhNhatGanDay"
                      style="width: 100%"
                    >
                      <div class="col-md-12 box-header-right">
                        <span>Sinh nhật gần đây</span>
                      </div>
                      <div class="col-md-12 box-content-right">
                        <div
                          class="scroll-outer"
                          style="
                            max-height: 40vh;
                            overflow-x: hidden;
                            overflow-y: auto;
                          "
                        >
                          <div class="scroll-inner">
                            <div
                              class="row"
                              v-for="(mn, indexn) in list_birthday_recently"
                              :key="indexn"
                              style="margin-bottom: 10px"
                            >
                              <div class="col-md-3 pt-2">
                                <span style="font-weight: bold">{{
                                  mn.day
                                }}</span>
                                <br />
                                <span style="color: gray; font-size: 13px">{{
                                  moment(new Date(mn.ngaySinhNhat)).format(
                                    "DD/MM",
                                  )
                                }}</span>
                              </div>
                              <div class="col-md-9">
                                <div
                                  v-for="(u, index) in mn.users"
                                  :key="index"
                                  style="float: left; margin: 5px"
                                >
                                  <Avatar
                                    v-tooltip.top="{
                                      value:
                                        u.full_name +
                                        '<br/>' +
                                        (u.tenChucVu || '') +
                                        '<br/>' +
                                        (u.tenToChuc || ''),
                                      escape: true,
                                    }"
                                    v-bind:label="
                                      u.avatar
                                        ? ''
                                        : u.last_name.substring(0, 1)
                                    "
                                    v-bind:image="basedomainURL + u.avatar"
                                    style="
                                      background-color: #2196f3;
                                      color: #ffffff;
                                      cursor: pointer;
                                    "
                                    :style="{
                                      background: bgColor[index % 7],
                                    }"
                                    class="mr-2"
                                    size="xlarge"
                                    shape="circle"
                                  />
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div
                    class="col-md-12"
                    style="padding: 0 7px; margin-top: 10px"
                  >
                    <div
                      class="right-side"
                      style="width: 100%"
                    >
                      <div class="col-md-12 box-header-right">
                        <span>Danh bạ</span>
                        <span class="count-ns-sn"
                          >{{ list_birthday_tomonthother.length }} người</span
                        >
                      </div>
                      <div
                        v-for="(m, index) in list_birthday_tomonthother"
                        :key="index"
                      ></div>
                      <div
                        class="col-md-12 box-content-right scroll-hover p-0"
                        :style="
                          'height:calc(100vh - 170px - ' +
                          (heightSN || 145) +
                          'px) !important'
                        "
                      >
                        <div
                          v-bind:id="m.user_id.replace('.', '')"
                          v-for="(m, index) in list_birthday_tomonthother"
                          :key="index"
                          style="
                            float: left;
                            padding: 5px;
                            width: 100%;
                            cursor: pointer;
                          "
                          @mouseover="ShowInForUser(m)"
                          @mouseleave="HidenInForUser(m)"
                          class="row-users flex"
                        >
                          <div
                            style="display: inline-block; position: relative"
                          >
                            <Avatar
                              v-bind:label="
                                m.avatar ? '' : m.last_name.substring(0, 1)
                              "
                              v-bind:image="basedomainURL + m.avatar"
                              style="
                                background-color: #2196f3;
                                color: #ffffff;
                                cursor: pointer;
                              "
                              :style="{
                                background: bgColor[index % 7],
                              }"
                              class="mr-2"
                              size="xlarge"
                              shape="circle"
                            />
                            <span
                              :class="m.status == 1 ? ' online' : ' offline '"
                            ></span>
                          </div>
                          <div style="">
                            <div
                              style="
                                font-size: 14px;
                                padding-left: 15px;
                                font-weight: 600;
                              "
                            >
                              {{ m.full_name }}
                            </div>
                            <div
                              style="
                                font-size: 13px;
                                padding-left: 15px;
                                color: gray;
                              "
                              v-if="m.display_birthday == true"
                            >
                              {{
                                m.birthday != null ? moment(new Date(m.birthday)).format(
                                  "DD/MM/YYYY",
                                ) : ""
                              }}
                            </div>
                            <!-- <span v-bind:class="m.user_id == GetNhanSu_ID ? ' display-info' : ''" style="display: none; float: right; font-size: 24px;padding-right: 5px;"  v-tooltip.top="'Nhắn tin'"><i class="pi pi-envelope text-3xl"></i></span>                          -->
                          </div>
                          <div
                            class="box-info-user"
                            v-bind:class="
                              m.user_id == GetNhanSu_ID ? ' display-info' : ''
                            "
                            :style="{ top: offset_top }"
                            style="display: none"
                          >
                            <div
                              style="
                                width: 110px;
                                height: 110px;
                                display: inline-block;
                                float: left;
                              "
                            >
                              <Avatar
                                v-bind:label="
                                  m.avatar ? '' : m.last_name.substring(0, 1)
                                "
                                v-bind:image="basedomainURL + m.avatar"
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  cursor: pointer;
                                "
                                :style="{
                                  background: bgColor[index % 7],
                                }"
                                class="mr-2"
                                size="xlarge"
                                shape="circle"
                              />
                            </div>
                            <div
                              style="
                                width: 260px;
                                display: inline-block;
                                float: right;
                              "
                            >
                              <div
                                class="col"
                                style="font-size: 16px; font-weight: 600"
                              >
                                {{ m.full_name }}
                              </div>
                              <div
                                class="col"
                                style="font-weight: 500"
                              >
                                Tên truy cập: {{ m.user_id }}
                              </div>
                              <div class="col">{{ m.tenChucVu }}</div>
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
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.col-md-12 {
  padding-left: 10px;
}
.d-lang-table {
  height: calc(100vh - 170px);
}
/*.center-side::-webkit-scrollbar {
            display: none;
        }*/
.display-info {
  display: inline-block !important;
}
.box-info-user {
  width: 400px;
  height: 130px;
  /*min-height: 150px;
        max-height: 200px;*/
  box-shadow: 0px 2px 6px 1px rgba(0, 0, 0, 0.2);
  background-color: #e6f0f8;
  position: absolute;
  z-index: 9999;
  /* display: none; */
  bottom: 99px;
  display: inline-block;
  position: fixed;
  right: 340px;
  border: 0;
  border: 1px solid #ccc;
  padding: 10px;
}
#ModalInFo {
  position: fixed;
  top: auto;
  right: 0;
  left: auto;
  bottom: 0;
}

.SinhnhatCtr {
  background-color: #fff;
  width: 100%;
}

.box-thang-nam .item-pnpage i {
  color: gray;
}

.box-thang-nam .item-pnpage i:hover {
  color: #0081d3;
}

/*style center*/
.center-side {
  width: calc(100% - 375px);
  float: left;
  margin: 5px;
  height: calc(100vh - 92px);
  padding: 5px;
  overflow-y: auto;
}

.center-side .box-header-center {
  padding: 5px 0 15px 0;
  color: #0081d3;
  font-weight: bold;
  border-bottom: 1px solid #dadada;
  border-top: 2px solid rgba(255, 255, 255, 0);
  font-size: 16px;
}

.center-side .box-homnay .row-box-homnay {
  margin-bottom: 15px !important;
  background-color: #fff;
  border-radius: 5px;
  margin: 1px 0;
  box-shadow: 0px 1px 3px 1px rgba(0, 0, 0, 0.2);
}

.center-side .box-homnay .box-sn-hn {
  padding: 10px 0 10px 10px;
  color: #ff662c;
  font-weight: bold;
  font-size: 16px;
}

.center-side .box-homnay .box-item-snhn {
  width: 240px;
  height: 100px;
  position: relative;
  margin: 5px 10px;
  float: left;
}

.center-side .box-homnay .box-item-snhn .img-user-sn {
  position: absolute;
  width: 40px;
  top: 0;
  left: 0;
}

.center-side .box-homnay .box-item-snhn .img-user-sn img {
  width: 40px;
  height: 40px;
  object-fit: cover;
  border-radius: 50%;
  border: 2px solid #ff662c;
  padding: 1px;
}

.center-side .box-homnay .box-item-snhn .info-user-sn {
  position: absolute;
  top: 0;
  left: 60px;
  font-size: 15px;
  line-height: 1.2;
}

.center-side .box-homnay .box-item-snhn .info-user-sn span.info-user-sn-name {
  font-weight: bold;
}

.center-side .box-homnay .box-item-snhn .info-user-sn span.info-user-sn-tuoi {
  font-size: 13px;
  color: gray;
}

.center-side .box-homnay .box-item-snhn .info-user-sn .btn-gui-loi-chuc {
  display: inline-block;
  margin: 5px 0;
  background-color: #f35369;
  padding: 5px 15px;
  color: #fff;
  border-radius: 3px;
  font-size: 14px;
  cursor: pointer;
}

.center-side .box-homnay .box-item-snhn .info-user-sn .btn-gui-loi-chuc:hover {
  color: #fff !important;
  /*font-weight: 600;*/
  text-decoration: underline;
}

.center-side .box-saptoi .row-box-saptoi {
  background-color: #fff;
  border-radius: 5px;
  margin: 1px 0;
  box-shadow: 0px 1px 3px 1px rgba(0, 0, 0, 0.2);
}

.center-side .box-saptoi .box-sn-hn {
  padding: 10px 0 10px 10px;
  color: #000;
  font-weight: bold;
  font-size: 16px;
}

.center-side .box-saptoi .box-item-snhn {
  width: 240px;
  height: 70px;
  position: relative;
  margin: 5px 10px;
  float: left;
}

.center-side .box-saptoi .box-item-snhn .img-user-sn {
  position: absolute;
  width: 40px;
  top: 0;
  left: 0;
}

.center-side .box-saptoi .box-item-snhn .img-user-sn img {
  width: 40px;
  height: 40px;
  object-fit: cover;
  border-radius: 50%;
  border: 1px solid darkgray;
}

.center-side .box-saptoi .box-item-snhn .info-user-sn {
  position: absolute;
  top: 3px;
  left: 70px;
  font-size: 15px;
  line-height: 1.2;
}

.center-side .box-saptoi .box-item-snhn .info-user-sn span.info-user-sn-name {
  font-weight: bold;
}

.center-side .box-saptoi .box-item-snhn .info-user-sn span.info-user-sn-tuoi {
  font-size: 13px;
  color: gray;
}

.center-side .box-saptoi .box-item-snhn .info-user-sn .btn-gui-loi-chuc {
  display: inline-block;
  margin: 5px 0;
  background-color: #8a48f7;
  padding: 5px 15px;
  color: #fff;
  border-radius: 3px;
  font-size: 14px;
}

.center-side .box-saptoi .img-user-sn {
  margin: 5px;
  float: left;
}

.center-side .box-saptoi .img-user-sn img {
  width: 40px;
  height: 40px;
  object-fit: cover;
  border-radius: 50%;
  border: 1px solid darkgray;
}

.center-side .box-saptoi .row-thang {
  background-color: #fff;
  margin: 20px 0;
  border-radius: 5px;
  padding-bottom: 15px;
  box-shadow: 0px 1px 3px 1px rgba(0, 0, 0, 0.2);
}

.center-side .box-saptoi .item-thang {
  font-weight: bold;
  color: #000;
  margin: 15px 0 5px 0;
  border-bottom: 1.2px solid #e2e2e2;
  padding-bottom: 13px;
  font-size: 16px;
}

/*style right*/
.right-side {
  width: 350px;
  float: right;
  margin: 5px;
  padding: 5px 10px;
  box-shadow: 0px 1px 3px 1px rgba(0, 0, 0, 0.2);
  background-color: #fff;
}

.right-side .box-header-right {
  padding: 5px 0 15px 0;
  color: #000;
  font-weight: bold;
  border-bottom: 1px solid #dadada;
  border-top: 2px solid rgba(255, 255, 255, 0);
  font-size: 16px;
}

.right-side .box-content-right {
  padding: 15px 15px 0 15px;
}

.right-side .box-content-right .box-filter {
  display: block;
  position: relative;
  width: 155px;
  height: 110px;
  border-radius: 5px;
  margin: 5px;
  float: left;
}

.right-side .box-content-right .box-filter:hover {
  opacity: 0.8;
}

.right-side .box-content-right .box-filter .box-icon-filter {
  display: block;
  position: absolute;
  left: 10px;
  top: 10px;
}

.right-side .box-content-right .box-filter .box-info-filter {
  display: block;
  position: absolute;
  color: #fff;
  font-size: 18px;
  left: 10px;
  bottom: 10px;
  font-weight: 500;
}

.active-btn-2 .dropdown-menu.show {
  min-width: 500px !important;
}

/* css buttom*/
.active-btn {
  cursor: pointer;
  background-color: #0078d4;
  padding: 8px !important;
  color: white !important;
  margin-left: 4px;
  transition: background-color 0.156s ease;
  border-radius: 2px;
  margin-right: 3px;
  font-weight: 500;
  display: inline-block;
}

.active-btn i,
.active-btn-2 i {
  font-size: 18px;
}

.active-btn:hover {
  background-color: #005a9e;
}

.active-btn-2 {
  cursor: pointer;
  display: inline-block;
  /*background-color: #0078d4;*/
  padding: 8px !important;
  color: #2196f3 !important;
  transition: background-color 0.156s ease;
  border-radius: 2px;
  margin-right: 3px;
  font-weight: 500;
}

.active-btn-2:hover {
  background-color: #e6f0f8;
}

.active-btn-2 .nav-item .dropdown-menu {
  top: 8px !important;
  left: -8px !important;
}

.active-btn-2 .nav-item .dropdown-menu.f-left {
  top: 8px !important;
  left: -40px !important;
}

.active-btn-2 .nav-item ul.dropdown-menu {
  padding: 0 !important;
}

.active-btn-2 .nav-item .dropdown-menu li span {
  font-weight: 400 !important;
}

.active-btn-3 {
  cursor: pointer;
  display: inline-block;
  /*background-color: #0078d4;*/
  padding: 0 8px !important;
  color: #2196f3 !important;
  transition: background-color 0.156s ease;
  border-radius: 2px;
  margin-right: 3px;
  font-weight: 500;
}
/*.back-ground-tab-button {
        background-color: #F3F2F1 !important;
    }*/
.active-row-congty {
  background-color: #fffdd9;
}

.view-true {
  background-color: #e6f0f8;
}

.scroll-hover {
  overflow: hidden !important;
}

.scroll-hover:hover {
  overflow-y: scroll !important;
}

.count-ns-sn {
  display: inline-block;
  float: right;
  margin-right: 15px;
  /* background-color: #6DD230; */
  color: #72777a;
  /* width: 30px;
        height: 30px;*/
  text-align: center;
  /*border-radius: 50%;*/
  padding-top: 4px;
  font-weight: 500;
  font-size: 14px;
}
span.online {
  position: absolute;
  display: block;
  width: 14px;
  height: 14px;
  background-color: rgb(98, 203, 0);
  border-radius: 50%;
  left: 27px;
  bottom: 0px;
  border: 2px solid #fff;
}
.col-12 {
  padding-right: 15px;
  padding-left: 15px;
}
.p-avatar {
  font-size: 1.1rem !important;
}
.box-item-snhn img {
  object-fit: cover;
  border: 2px solid #ff662c;
}
.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
  visibility: visible;
}
.scroll-outer {
  visibility: hidden;
  /* max-height: 435px;
    overflow-y: auto; */
}
.row-users:hover {
  background-color: #e6f0f8;
}
.p-avatar.p-avatar-xl {
  width: 40px;
  height: 40px;
}
.box-homnay .p-avatar.p-avatar-xl {
  border: 2px solid #ff662c;
}
.v-tooltip__content {
  font-size: 52px !important;
  opacity: 1 !important;
  display: block !important;
  text-align: center !important;
}
.custom-error .p-tooltip-text {
  background-color: red !important;
  color: red !important;
}
.custom-error.p-tooltip-right .p-tooltip-arrow {
  border-right-color: red !important;
  background-color: red !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.box-info-user) {
  .p-avatar.p-avatar-xl {
    width: 100% !important;
    height: 100% !important;
  }
  .p-avatar-text {
    font-size: 3rem;
  }
}
</style>
<style lang="scss" scoped>
@import "bootstrap/dist/css/bootstrap.min.css";
</style>
