<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
import { VuemojiPicker } from "vuemoji-picker";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const width = ref(window.screen.width);
const router = inject("router");
const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const checkDel = ref(false);
let id = store.state.user.user_id;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const panel_gif = ref();
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const basedomainURL = fileURL;
const currentDay = new Date();
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
    if (ngay.getDate() === toDay.value.Ngay - 1) {
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
  loadCMT(true);
};
//emoji
const panelEmoij1 = ref();
const panelEmoij2 = ref();
const panelEmoij3 = ref();
const panelCalendar = ref();
const panelEmoij4 = ref();
let filecoments = [];
const listFileComment = ref([]);
const comment = ref("");

const checkFileComment = ref(false);
//tung
//end tung
const comment_zone_main = ref();
let line1 = "";
let line = "";
const showEmoji = (event, check) => {
  if (check == 1) panelEmoij1.value.toggle(event);
  if (check == 2) panelEmoij2.value.toggle(event);
  if (check == 3) panelEmoij3.value.toggle(event);
  if (check == 4) panelEmoij4.value.toggle(event);
  if (check == 5) panelCalendar.value.toggle(event);
};
const Change = (event) => {
  line = event.range.index ? event.range.index : null;
};
const handleEmojiClick = (event) => {
  comment.value = comment.value.replace("<p>", "").replace("</p>", "");
  line1 = line;
  let str1 = comment.value.slice(0, line1);
  let str2 = comment.value.slice(line1);
  if (comment.value)
    comment.value =
      line1 > 0 ? str1 + event.unicode + str2 : comment.value + event.unicode;
  else comment.value = event.unicode;
  comment.value = comment.value.replace("<p>", "").replace("</p>", "");
  comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
};

const chonanh = (id) => {
  document.getElementById(id).click();
};

const handleFileUploadComment = (event) => {
  filecoments = [];
  filecoments = event.target.files;
  if (filecoments.length > 10) {
    swal.fire({
      title: "Thông báo",
      text: "Bạn chỉ có thể chọn tối đa 10 ảnh/gif!",
      icon: "error",
      confirmButtonText: "OK",
    });
    filecoments = [];
  }
  let le = filecoments.length;
  if (filecoments) {
    checkFileComment.value = true;
    for (let index = 0; index < le; index++) {
      const element = filecoments[index];

      var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
      //Kiểm tra định dạng
      if (allowedExtensions.exec(element.name)) {
        listFileComment.value.push({
          data: element,
          src: URL.createObjectURL(element),
          checkimg: true,
        });
        if (listFileComment.value.length > 10) {
          swal.fire({
            title: "Thông báo",
            text: "Bạn chỉ có thể chọn tối đa 10 ảnh/gif!",
            icon: "error",
            confirmButtonText: "OK",
          });
        }
        URL.revokeObjectURL(element);
      } else {
        swal.fire({
          title: "Thông báo",
          text: "Ảnh không đúng định dạng, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        filecoments = [];
      }
    }
  }
};
const delImgComment = (value) => {
  checkDel.value = true;
  let arrImg = [];
  let le = filecoments.length;
  for (let index = 0; index < le; index++) {
    const element = filecoments[index];
    if (element != value) {
      arrImg.push(element);
    }
  }
  filecoments = arrImg;
  listFileComment.value = listFileComment.value.filter((x) => x.data != value);
  listFileComment.value = listFileComment.value.filter((x) => x != value);
};

const options = ref({
  IsNext: true,
  sort: "is_order desc",
  SearchText: "",
  PageNo: 0,
  PageSize: 8,
  loading: true,
  totalRecords: null,
  loadingCMT: true,
  totalCMTRecords: null,
});

const isFirst = ref(false);
const datalists = ref();
const loadData = (rf) => {
  options.value.loading = true;
  if (rf) {
    axios
      .post(
        // eslint-disable-next-line no-undef
        baseURL + "/api/TaskProc/getTaskData",
        {
          str: encr(
            JSON.stringify({
              proc: "birthday_gif_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
              ],
            }),
            SecretKey,
            cryoptojs,
          ).toString(),
        },
        config,
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];

        datalists.value = data;
        let data1 = JSON.parse(response.data.data)[1];
        options.value.totalRecords = data1[0].totalRecords;
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo",
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
};

const showGif = (event) => {
  panel_gif.value.toggle(event);
};
const nextPage = (event) => {
  options.value.PageNo = event.page;
  options.value.PageSize = event.rows;
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  loadData(true);
};
const AddToTemp = (event) => {
  checkFileComment.value = true;
  if (listFileComment.value == null) {
    listFileComment.value = [];
  }
  listFileComment.value.push(event.gif);
  if (listFileComment.value.length > 10) {
    swal.fire({
      title: "Thông báo",
      text: "Bạn chỉ có thể chọn tối đa 10 ảnh/gif!",
      icon: "error",
      confirmButtonText: "OK",
    });
    listFileComment.value = [];
  }
};
const listCMT = ref();
const loadCMT = (rf) => {
  options.value.loadingCMT = true;
  if (rf) {
    axios
      .post(
        // eslint-disable-next-line no-undef
        baseURL + "/api/TaskProc/getTaskData",
        {
          str: encr(
            JSON.stringify({
              proc: "BirthDay_Cmt_List ",
              par: [
                { par: "user", va: store.state.birthDay.user_id },
                { par: "myDate", va: currentDay },
              ],
            }),
            SecretKey,
            cryoptojs,
          ).toString(),
        },
        config,
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((x) => {
          if (x.image) {
            let img = [];
            x.image.split(",").forEach((z) => {
              img.push(z);
            });

            x.image = img;
          } else {
            x.image = null;
          }
        });
        data
          .filter((x) => x.contents.includes("Portal"))
          .forEach((element) => {
            element.contents = element.contents.replace("30vw", "auto");
          });
        listCMT.value = data;
        let data1 = JSON.parse(response.data.data)[1];
        options.value.totalCMTRecords = data1[0].totalCMTRecords;
        options.value.loadingCMT = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");

        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo",
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
};
const idv = ref();
const addComment = () => {
  if (
    comment.value == "" &&
    comment.value == null &&
    comment.value == "<p><br></p>" &&
    listFileComment.value.length != 0
  )
    return;
  else {
    let formData = new FormData();
    if (edit.value == true) {
      formData.append("id", JSON.stringify(idv.value));
    }
    let bugComment = {
      contents: "<body>" + comment.value + "</body>",
    };

    if (filecoments != null)
      for (var i = 0; i < filecoments.length; i++) {
        let file = filecoments[i];
        formData.append("url_file", file);
      }
    let gif = [];
    if (listFileComment.value != null && listFileComment.value != []) {
      if (edit.value == true) {
        listFileComment.value.forEach((x) => {
          if (!x.data) {
            if (x.includes("Portals")) {
              gif.push(x);
            }
          }
        });
      } else {
        listFileComment.value.forEach((x) => {
          if (!x.data) {
            if (x.includes("Portals/Gif")) {
              gif.push(x);
            }
          }
        });
      }
    }
    formData.append("gif", JSON.stringify(gif));
    formData.append("comment", JSON.stringify(bugComment));
    formData.append(
      "receive_user",
      JSON.stringify(store.state.birthDay.user_id),
    );

    axios({
      method: edit.value ? "put" : "post",
      url:
        baseURL +
        `/api/BirthDay_CMT/${
          edit.value ? "update_BD_CMT" : "add_Birthday_CMT"
        }`,
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success(
            edit.value
              ? "Cập nhật lời chúc thành công!"
              : "Thêm mới lời chúc thành công!",
          );
          comment.value = "";
          comment_zone_main.value.setHTML("");
          filecoments = [];
          listFileComment.value = [];
          edit.value = false;
          loadCMT(true);
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch(() => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};
const DeleteItem = (vl) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá lời chúc này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        axios
          .delete(baseURL + "/api/BirthDay_CMT/delete_BD_CMT", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: vl != null ? [vl] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá lời chúc thành công!");
              loadCMT(true);
            } else {
              swal.fire({
                title: "Error!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                title: "Error!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
  edit.value = false;
};

const edit = ref(false);
const EditItem = (vl) => {
  edit.value = edit.value == false ? true : false;
  filecoments = [];
  listFileComment.value = [];
  comment.value = vl.contents
    .replace("<body><p>", "")
    .replace("</p></body>", "");
  checkFileComment.value = true;
  comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
  filecoments = vl.image;
  listFileComment.value = vl.image;
  idv.value = vl.id;

  if (edit.value == false) {
    listFileComment.value = [];
    filecoments = [];
    comment.value = null;
    comment_zone_main.value.setHTML(comment.value);
  }
};

onMounted(() => {
  listFileComment.value = [];
  if (store.state.birthDay.user_id == null) {
    router.push({
      path: "/birthday",
    });
  }
  loadCMT(true);
  bgColor;
  Get_BirthdayUserRecently();
  Get_BirthdayUserToday();
  Get_BirthdayUserToMonthOther();
  loadData(true);
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
                                                @error="
                                                  $event.target.src =
                                                    basedomainURL +
                                                    '/Portals/Image/nouser1.png'
                                                "
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
                                                v-if="
                                                  store.state.user.user_id !=
                                                  m.user_id
                                                "
                                                @click="ChucMung(m)"
                                                class="btn-gui-loi-chuc"
                                              >
                                                Gửi lời chúc
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
                                    <div
                                      class="col-md-12 box-homnay"
                                      v-if="list_birthday_today.length > 0"
                                    >
                                      <div
                                        class="row p-3 row-box-homnay"
                                        style="width: 100%"
                                        :style="
                                          store.state.user.user_id !=
                                          store.state.birthDay.user_id
                                            ? ''
                                            : 'max-height: calc(75vh - 1.768rem ) !important'
                                        "
                                      >
                                        <span
                                          class="format-center"
                                          style="
                                            padding-bottom: 0.5rem;
                                            color: #ff662c;
                                            font-weight: bold;
                                            font-size: 20px;
                                            max-height: 1.5rem;
                                          "
                                        >
                                          Lời chúc tới:
                                          {{ store.state.birthDay.full_name }}
                                        </span>
                                        <div
                                          class="scroll-outer"
                                          style="
                                            width: 100%;
                                            padding: 0;
                                            overflow-x: hidden;
                                            overflow-y: auto;
                                          "
                                          :style="
                                            store.state.user.user_id !=
                                            store.state.birthDay.user_id
                                              ? listFileComment == null
                                                ? checkDel == true
                                                  ? 'max-height: calc(53vh - 1.75rem ) !important'
                                                  : 'max-height: calc(53vh - .75rem) !important'
                                                : 'max-height: calc(53vh - .75rem) !important'
                                              : 'max-height: calc(71vh - 1.768rem ) !important'
                                          "
                                        >
                                          <div class="scroll-inner">
                                            <div
                                              v-for="(item, index) in listCMT"
                                              :key="index"
                                              class="mr-2 relative"
                                              style="
                                                align-items: unset !important;
                                                justify-content: unset !important;
                                              "
                                            >
                                              <div class="col-md-12 box-sn-hn">
                                                <div
                                                  class="row-box-homnay myclass"
                                                  style="
                                                    background-color: blanchedalmond;
                                                  "
                                                >
                                                  <div
                                                    class="p-col-12 flex align-items-center m-0 p-0"
                                                    style="
                                                      border-bottom: 1px solid
                                                        #dadada !important;
                                                      border-bottom-color: #dadada;
                                                      color: black;
                                                    "
                                                  >
                                                    <div
                                                      class="p-col-2 lg:p-col-3 pl-4 m-0 flex format-center"
                                                    >
                                                      <Avatar
                                                        @error="
                                                          $event.target.src =
                                                            basedomainURL +
                                                            '/Portals/Image/nouser1.png'
                                                        "
                                                        v-bind:label="
                                                          item.AvtNguoiGui
                                                            ? ''
                                                            : item.NguoiGui.split(
                                                                ' ',
                                                              )
                                                                .at(-1)
                                                                .substring(0, 1)
                                                        "
                                                        v-bind:image="
                                                          basedomainURL +
                                                          item.AvtNguoiGui
                                                        "
                                                        style="
                                                          background-color: #2196f3;
                                                          color: #ffffff;
                                                        "
                                                        :style="{
                                                          background: bgColor,
                                                        }"
                                                        class="format-center"
                                                        size="normal"
                                                        shape="circle"
                                                      />
                                                      <span
                                                        class="px-2 mx-2 text-xl format-center"
                                                      >
                                                        {{
                                                          item.NguoiGui
                                                        }}</span
                                                      >
                                                    </div>
                                                    <div
                                                      class="px-2 mx-2 mx-4 text-2xl format-center"
                                                    >
                                                      <i
                                                        class="pi pi-caret-right format-center"
                                                      >
                                                      </i>
                                                    </div>
                                                    <div
                                                      class="p-col-2 lg:p-col-3 p-0 px-0 m-0 flex align-items-center"
                                                    >
                                                      <Avatar
                                                        @error="
                                                          $event.target.src =
                                                            basedomainURL +
                                                            '/Portals/Image/nouser1.png'
                                                        "
                                                        v-bind:label="
                                                          item.AvtNguoiNhan
                                                            ? ''
                                                            : item.NguoiNhan.split(
                                                                ' ',
                                                              )
                                                                .at(-1)
                                                                .substring(0, 1)
                                                        "
                                                        v-bind:image="
                                                          basedomainURL +
                                                          item.AvtNguoiNhan
                                                        "
                                                        :style="{
                                                          background:
                                                            bgColor[index % 7],
                                                        }"
                                                        class="align-items-center"
                                                        size="normal"
                                                        shape="circle"
                                                      />
                                                      <span class="m-2 text-xl">
                                                        {{ item.NguoiNhan }}
                                                      </span>
                                                    </div>

                                                    <div
                                                      class="absolute top-auto right-0 text-white font-bold flex align-items-center justify-content-center"
                                                      v-if="
                                                        item.created_by ==
                                                        store.state.user.user_id
                                                      "
                                                    >
                                                      <button
                                                        class="mybutton4 top-auto right-0 text-white font-bold flex align-items-center justify-content-center w-3rem h-2rem"
                                                        style="
                                                          background-color: blanchedalmond;
                                                          color: red !important;
                                                        "
                                                        @click="EditItem(item)"
                                                      >
                                                        <i
                                                          class="fp pi pi-pencil"
                                                        >
                                                        </i>
                                                      </button>
                                                      <button
                                                        class="mybutton4 top-auto right-0 text-white font-bold flex align-items-center justify-content-center w-3rem h-2rem"
                                                        style="
                                                          background-color: blanchedalmond;
                                                          color: red !important;
                                                        "
                                                        @click="
                                                          DeleteItem(item.id)
                                                        "
                                                      >
                                                        <i
                                                          class="fp pi pi-trash"
                                                        >
                                                        </i>
                                                      </button>
                                                    </div>
                                                  </div>
                                                  <div
                                                    class="p-col-12 w-full format-center align-items-center p-0 m-0"
                                                    style="
                                                      word-wrap: break-word;
                                                    "
                                                  >
                                                    <div
                                                      class="p-col-12 m-0 text-1xl p-0 font-light"
                                                      style="color: black"
                                                      v-if="
                                                        item.contents !=
                                                        '<body><p><br></p></body>'
                                                      "
                                                      v-html="item.contents"
                                                    ></div>
                                                  </div>
                                                  <div
                                                    v-if="item.image != null"
                                                    class="p-col-12 format-center p-0 m-0 pb-2"
                                                    style="
                                                      display: grid;
                                                      grid-template-columns: auto auto auto auto auto;
                                                    "
                                                  >
                                                    <div
                                                      class="px-2 pt-2 format-center"
                                                      v-for="itemss in item.image"
                                                      :key="itemss"
                                                    >
                                                      <Image
                                                        :src="
                                                          itemss != null
                                                            ? basedomainURL +
                                                              itemss
                                                            : ''
                                                        "
                                                        preview
                                                        :imageStyle="
                                                          width < 1900
                                                            ? 'width: 4rem; height: 4rem'
                                                            : 'width: 10rem; height: 10rem'
                                                        "
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
                                    <div
                                      class="col-md-12 box-saptoi"
                                      style="max-height: 10vh"
                                    >
                                      <div
                                        class="row row-box-saptoi"
                                        v-if="
                                          store.state.birthDay.user_id !=
                                          store.state.user.user_id
                                        "
                                      >
                                        <div
                                          class="col-md-12 box-sn-hn"
                                          v-if="
                                            store.state.birthDay.user_id !=
                                            store.state.user.user_id
                                          "
                                        >
                                          <div
                                            class="text-2xl format-center"
                                            style="
                                              justify-content: flex-start;
                                              color: #ff662c;
                                            "
                                          >
                                            <span v-if="edit == false">
                                              Gửi lời chúc <span>tới: </span>
                                            </span>
                                            <span v-else
                                              >Sửa lời chúc tới:
                                            </span>

                                            <div class="pl-2">
                                              <Avatar
                                                @error="
                                                  $event.target.src =
                                                    basedomainURL +
                                                    '/Portals/Image/nouser1.png'
                                                "
                                                v-bind:label="
                                                  store.state.birthDay.avatar
                                                    ? ''
                                                    : store.state.birthDay.full_name
                                                        .split(' ')
                                                        .at(-1)
                                                        .substring(0, 1)
                                                "
                                                v-bind:image="
                                                  basedomainURL +
                                                  store.state.birthDay.avatar
                                                "
                                                style="
                                                  background-color: #2196f3;
                                                  color: #ffffff;
                                                "
                                                :style="{
                                                  background:
                                                    bgColor[index % 7],
                                                }"
                                                class="p-1"
                                                size="large"
                                                shape="circle"
                                                v-if="
                                                  store.state.birthDay.avatar
                                                "
                                              />
                                            </div>
                                            <span class="info-user-sn-name">
                                              {{
                                                store.state.birthDay.full_name
                                              }}
                                            </span>
                                          </div>
                                        </div>

                                        <div class="">
                                          <div class="">
                                            <div class="grid">
                                              <div
                                                class="col-12 p-0 m-0 font-bold pl-2"
                                                style="
                                                  font-weight: bold;
                                                  font-size: 16px;
                                                  margin: 10px 0;
                                                  border-top: 1px solid #f5f5f5;
                                                  padding-top: 15px;
                                                  color: #2196f3;
                                                "
                                                v-if="
                                                  listFileComment.length > 0
                                                "
                                              >
                                                File đính kèm
                                              </div>
                                              <div
                                                class="col-12 flex"
                                                v-if="
                                                  listFileComment.length > 0
                                                "
                                                style="
                                                  max-width: 70vw;
                                                  height: auto;
                                                  display: flex;
                                                  flex-wrap: wrap;
                                                "
                                              >
                                                <div
                                                  v-for="(
                                                    item, index
                                                  ) in listFileComment"
                                                  :key="index"
                                                  class="mr-2 relative"
                                                  style=""
                                                >
                                                  <Button
                                                    @click="
                                                      delImgComment(
                                                        item.data
                                                          ? item.data
                                                          : item,
                                                      )
                                                    "
                                                    icon="pi pi-times"
                                                    class="p-button-rounded p-button-text p-button-plain absolute top-0 right-0 w-1rem h-1rem"
                                                  ></Button>
                                                  <img
                                                    v-if="item.checkimg"
                                                    :src="item.src"
                                                    :alt="item.data.name"
                                                    style="
                                                      width: 100px;
                                                      height: 100px;
                                                      object-fit: contain;
                                                    "
                                                  />
                                                  <img
                                                    v-else
                                                    :src="basedomainURL + item"
                                                    style="
                                                      width: 100px;
                                                      height: 100px;
                                                      object-fit: contain;
                                                    "
                                                    alt=""
                                                  />
                                                </div>
                                              </div>
                                              <div class="col-12 flex">
                                                <div
                                                  class="col-12 border-1 border-round-xs border-600 flex"
                                                  style="border-radius: 5px"
                                                >
                                                  <div
                                                    class="border-0 col-10 p-0 m-0"
                                                    style="max-width: 80vw"
                                                  >
                                                    <QuillEditor
                                                      ref="comment_zone_main"
                                                      placeholder="Nhập nội dung bình luận..."
                                                      contentType="html"
                                                      :content="comment"
                                                      v-model:content="comment"
                                                      theme="bubble"
                                                      @selectionChange="
                                                        Change($event)
                                                      "
                                                    />
                                                  </div>
                                                  <div class="col-2 p-0 m-0">
                                                    <div
                                                      class="format-center flex col-12 p-0 m-0"
                                                    >
                                                      <!-- v-clickoutside="onHideEmoji" -->
                                                      <Button
                                                        class="p-button-text p-button-plain col-3 mx-1 format-center w-3rem h-3rem"
                                                        @click="showGif($event)"
                                                      >
                                                        <img
                                                          alt="logo"
                                                          :src="
                                                            basedomainURL +
                                                            '/Portals/BirthDay_CMT/200 (1).gif'
                                                          "
                                                          width="20"
                                                          height="20"
                                                        />
                                                      </Button>
                                                      <Button
                                                        class="p-button-text p-button-plain col-3 mx-1 format-center w-3rem h-3rem"
                                                        @click="
                                                          showEmoji($event, 1)
                                                        "
                                                      >
                                                        <img
                                                          alt="logo"
                                                          src="/src/assets/image/smile.png"
                                                          width="20"
                                                          height="20"
                                                        />
                                                      </Button>

                                                      <Button
                                                        class="p-button-text p-button-plain col-3 mx-1 w-3rem h-3rem"
                                                        @click="
                                                          chonanh('anhcongviec')
                                                        "
                                                      >
                                                        <img
                                                          alt="logo1"
                                                          src="/src/assets/image/imageicon.png"
                                                          height="24"
                                                        />
                                                      </Button>
                                                      <Button
                                                        icon="pi pi-send pt-1 pr-1 font-bold"
                                                        class="p-button-text p-button-plain col-3 mx-1 w-3rem h-3rem"
                                                        style="
                                                          background-color: ;
                                                          color: black;
                                                        "
                                                        @click="addComment()"
                                                      />
                                                      <input
                                                        class="hidden"
                                                        id="anhcongviec"
                                                        type="file"
                                                        multiple="true"
                                                        accept="image/*"
                                                        @change="
                                                          handleFileUploadComment
                                                        "
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
                          class="row"
                          v-for="(mn, indexn) in list_birthday_recently"
                          :key="indexn"
                          style="margin-bottom: 10px"
                        >
                          <div class="col-md-3 p-1">
                            <span style="font-weight: bold">{{ mn.day }}</span
                            ><br />
                            <span style="color: gray; font-size: 13px">{{
                              moment(new Date(mn.ngaySinhNhat)).format("DD/MM")
                            }}</span>
                          </div>
                          <div class="col-md-9">
                            <div
                              v-for="(u, index) in mn.users"
                              :key="index"
                              style="float: left; margin: 5px"
                            >
                              <Avatar
                                @error="
                                  $event.target.src =
                                    basedomainURL + '/Portals/Image/nouser1.png'
                                "
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
                                  u.avatar ? '' : u.last_name.substring(0, 1)
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
                        <span
                          class="count-ns-sn right-0"
                          style="float: right !important"
                        >
                          {{ list_birthday_tomonthother.length }} người
                        </span>
                      </div>
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
                              @error="
                                $event.target.src =
                                  basedomainURL + '/Portals/Image/nouser1.png'
                              "
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
                            >
                              {{
                                moment(new Date(m.birthday)).format(
                                  "DD/MM/YYYY",
                                )
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
                                @error="
                                  $event.target.src =
                                    basedomainURL + '/Portals/Image/nouser1.png'
                                "
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
  <OverlayPanel
    class="p-0"
    ref="panelEmoij1"
    appendTo="body"
    :showCloseIcon="false"
    id="overlay_panelEmoij1"
  >
    <VuemojiPicker @emojiClick="handleEmojiClick" />
  </OverlayPanel>
  <OverlayPanel
    class="p-0"
    ref="panel_gif"
    appendTo="body"
    :showCloseIcon="false"
    id="overlay_gif"
  >
    <DataView
      :value="datalists"
      layout="grid"
      :paginator="options.totalRecords > options.PageSize"
      :rows="options.PageSize"
      @page="nextPage($event)"
      :totalRecords="options.totalRecords"
      :lazy="true"
      :loading="options.loading"
      :pageLinkSize="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink  NextPageLink LastPageLink"
      responsiveLayout="scroll"
      style="max-width: 18vw"
      :rowHover="true"
    >
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          v-if="!isFirst"
        >
          <img
            src="../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
      <template #grid="data">
        <div class="col-3 md:col-3">
          <div class="product-grid-item">
            <div class="product-grid-item-content">
              <img
                :width="width < 1920 ? '45' : '60'"
                :height="width < 1920 ? '45' : '60'"
                alt=" "
                v-bind:src="
                  data.data.gif
                    ? basedomainURL + data.data.gif
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />
              <button
                class="mybutton"
                @click="AddToTemp(data.data)"
              ></button>
            </div>
          </div>
        </div>
      </template>
    </DataView>
  </OverlayPanel>
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
  margin-right: 15px;
  color: #72777a;
  text-align: center;
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
@import "bootstrap/dist/css/bootstrap.min.css";
</style>
<style>
.fp {
  font-size: large;
}
.mybutton4 {
  color: red;
  border: none;
  cursor: pointer;
  font-size: xx-small !important;
}
/* tung */

.list-bugs-item {
  border: solid #e9ecef;
  border-width: 0 0 1px 0;
  cursor: pointer;
}

.row.true {
  background-color: rgb(190, 211, 245) !important;
}
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-treetable) {
  .p-treetable-tbody > tr > td {
    padding: 0;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel) {
  .p-panel-header {
    padding: 0;
  }
}
</style>
<!-- //Style của gif picker -->
<style>
.product-grid-item-content {
  position: relative;
  color: white;
}
.fp {
  font-size: 175%;
}
.product-grid-item-content:hover .mybutton {
  display: block;
}
@media only screen and (max-width: 2560px) {
  .mybutton {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background-color: rgba(255, 255, 255, 0.5);
    color: red;
    padding: 2.5rem;
    border-color: rgba(0, 0, 0, 0.5);
    cursor: pointer;
    display: none;
  }
}

@media only screen and (max-width: 1920px) {
  .mybutton {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background-color: rgba(255, 255, 255, 0.5);
    color: red;
    padding: 2.5rem;
    border-color: rgba(0, 0, 0, 0.5);
    cursor: pointer;
    display: none;
  }
}
@media only screen and (max-width: 1420px) {
  .mybutton {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background-color: rgba(255, 255, 255, 0.5);
    color: red;
    padding: 2rem;
    border-color: rgba(0, 0, 0, 0.5);
    cursor: pointer;
    display: none;
    font-size: xx-small;
  }
}
</style>
<style lang="scss" scoped>
.card {
  background: #ffffff;
  padding: 2rem;
  box-shadow: 0 2px 1px -1px rgba(0, 0, 0, 0.2), 0 1px 1px 0 rgba(0, 0, 0, 0.14),
    0 1px 3px 0 rgba(0, 0, 0, 0.12);
  border-radius: 4px;
  margin-bottom: 2rem;
}

::v-deep(.product-grid-item) {
  margin: 0.5rem;
  border: 1px solid var(--surface-border);

  .product-grid-item-top,
  .product-grid-item-bottom {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  img {
    box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
    margin: 0rem 0;
  }

  .product-grid-item-content {
    text-align: center;
  }

  .product-price {
    font-size: 1.5rem;
    font-weight: 600;
  }
}
</style>
