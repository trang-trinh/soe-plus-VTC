<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required, maxLength, minLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedVideos = ref();
const checkDelList = ref(false);
const isFirstVideo = ref(false);
const isDisplayAvt = ref(false);
var FileNameCovert = "";
// var $ = require( "jquery" );
const rules = {
  title: {
    required,
  },
  title: {
    required,
    maxLength: maxLength(500),
  },
};
const taskDateFilter = ref();
const menu_ID = ref();
const menu_IDNode = ref();
const menu_IDNodeADD = ref();
const isHiddenFile = ref(false);
const first = ref(1);

//Lọc
const filterButs = ref();
const checkFilter = ref(false);
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const reFilterVideo = () => {
  checkFilter.value = false;
  options.value.status = null;
  options.value.type_video = null;
  loadData(true);
};
const filterVideo = () => {
  checkFilter.value = true;
  loadData(true);
};
//Refresh
const onRefersh = () => {
  options.value = {
    IsNext: true,
    search: "",
    pageno: 1,
    pagesize: 20,
    user_id: store.getters.user.user_id,
    status: null,
    tenstatus: "",
  };
  first.value = 1;
  checkFilter.value = false;
  filterTrangthai.value = null;
  options.value.status = null;
  showFilter.value = false;
  isDynamicSQL.value = false;
  selectedVideos.value = [];
  loadData(true);
};
// Upload, remove file
let files = [];
const onUploadFile = (event) => {
  if (isUpdateVideo.value) {
    isHiddenFile.value = true;
  }
  event.files.forEach((element) => {
    files[0] = element;
  });
};
const removeFile = (event) => {
  files = files.filter((a) => a != event.file);
};
const deleteFileCode = (value) => {
  video.value.path = null;
  files = [];
};
//Lấy file ảnh
const chonanh = (id) => {
  document.getElementById(id).click();
};
let avts = [];
const handleFileUpload = (event) => {
  avts = event.target.files;
  isDisplayAvt.value = true;
  var output = document.getElementById("logoLang");
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Xóa video

const delVideo = (video) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá video này không!",
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
          .delete(baseURL + "/api/video_main/delete_video", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: video != null ? [video.video_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá video thành công!");
              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo!",
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  title: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  start_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  is_hot: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
  status: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
});
//Phân trang dữ liệu
const isPaginator = ref(false);
const onPage = (event) => {
  if (event.rows != options.value.pagesize) {
    options.value.pagesize = event.rows;
  }

  options.value.pageno = event.page + 1;
  loadData();
};
const filterSQL = ref([]);
const isDynamicSQL = ref(false);
const loadDataSQL = () => {
  let data = {
    id: null,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filtervideo_main", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];

      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_sort = i + 1;
          if (!element.created_date_show) element.created_date_show = null;
          element.created_date_show = moment(
            new Date(element.created_date)
          ).format("DD/MM/YYYY");
        });

        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecords = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console loadData",
        controller: "Video.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Sort
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField != "video_id") {
    options.value.sort +=
      ",video_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  isDynamicSQL.value = true;
  loadData();
};
const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key,
        filteroperator: value.operator,
        filterconstraints: value.constraints,
      };
      if (value.value && value.value.length > 0) {
        obj.filteroperator = value.matchMode;
        obj.filterconstraints = [];
        value.value.forEach(function (vl) {
          obj.filterconstraints.push({ value: vl[obj.key] });
        });
      } else if (value.matchMode) {
        obj.filteroperator = "and";
        obj.filterconstraints = [value];
      }
      if (
        obj.filterconstraints &&
        obj.filterconstraints.filter((x) => x.value != null).length > 0
      )
        filterSQL.value.push(obj);
    }
  }

  options.value.pageno = 1;
  first.value = 1;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadData(true);
};
//DropDown

const onDropDown = (value) => {
  let data = {
    IntID: value.video_id,
    TextID: value.video_id + "",
    IntTrangthai: value.status,
    BitTrangthai: false,
  };
  axios
    .put(baseURL + "/api/video_main/update_status", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
        loadData(false);
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại",
          icon: "error",
          confirmButtonText: "OK",
        });
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
    });
};
//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.video_id,
    TextID: value.video_id + "",
    IntTrangthai: 0,
    BitTrangthai: value.is_hot,
    check: true,
  };
  axios
    .put(baseURL + "/api/video_main/update_ishot", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật tin thành công!");
        loadData(false);
      } else {
        console.log("LỖI A:", response);
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
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
    });
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedVideos.length);

  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa danh sách video này không!",
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
        selectedVideos.value.forEach((item) => {
          listId.push(item.video_id);
        });
        axios
          .delete(baseURL + "/api/video_main/delete_video", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá video thành công!");
              checkDelList.value = false;

              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo!",
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const delAvatar = () => {
  avts = [];
  isDisplayAvt.value = false;
  var output = document.getElementById("logoLang");
  output.src = basedomainURL + "/Portals/Image/noimg.jpg";
  video.value.image = null;
};
//Lấy file video

const toast = useToast();
const isFirst = ref(true);
const datalists = ref();
const isUpdateVideo = ref(false);
const sttVideo = ref(1);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//ADD log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const submitted = ref(false);
const options = ref({
  IsNext: true,
  sort: "video_id DESC",
  search: "",
  pageno: 1,
  pagesize: 20,

  loading: true,
  totalRecords: null,
  status: null,
  type_video: null,
});
const video = ref({
  is_order: 1,
  title: "",
  des: "",
  contents: "",
  image: "",
  is_hot: false,
  IsLang: store.getters.langid,
  Menu_ID: store.state.idVideo,
  video_type: 0,
  key_words: "",
  IsWriter: "",
  start_date: "",
  end_date: null,
  status: false,
});
const v$ = useVuelidate(rules, video);
const options_status = ref([
  { name: "Chưa duyệt", code: 1 },
  { name: "Đã duyệt", code: 2 },
  { name: "Đã đóng", code: 3 },
  { name: "Không duyệt", code: 4 },
]);
const options_type = ref([
  { name: "File tải lên", code: 1 },
  { name: "Youtube", code: 0 },
]);
const danhMuc = ref();
//METHOD
const displayDetails = ref(false);
const openDetails = (data) => {
  displayDetails.value = true;
  axios
    .post(
      baseURL + "/api/video_main/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "video_main_get",
            par: [{ par: "video_id", va: data.video_id }],
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
        video.value = data[0];
      } else video.value = store.getters.video;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const closeDetails = () => {
  displayDetails.value = false;
  video.value = {};
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/video_main/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "video_main_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "key_words", va: options.value.key_words },
              { par: "search", va: options.value.search },
              { par: "status", va: options.value.status },
              { par: "type_video", va: options.value.type_video },
            ],
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
        options.value.totalRecords = data[0].totalRecords;
      } else options.value.totalRecords = 0;
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
    });
};
var urlBase64 = "";
const saveVideo = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (
    (video.value.is_file_upload && files.length == 0 && !isUpdateVideo.value) ||
    (isUpdateVideo.value &&
      video.value.is_file_upload &&
      files.length == 0 &&
      isHiddenFile.value) ||
    (isUpdateVideo.value &&
      video.value.is_file_upload &&
      video.value.path == null &&
      !isHiddenFile.value)
  ) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng upload video!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  } else if (!video.value.is_file_upload && !video.value.link) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui nhập đường dẫn video!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (!video.value.is_file_upload) files = [];
  else {
    video.value.link = null;
    video.value.id_youtube = null;
  }
  if (video.value.link && !video.value.is_file_upload) {
    if (video.value.link.includes("watch?v=")) {
      video.value.id_youtube = video.value.link.substring(
        video.value.link.indexOf("watch?v=") + 8,
        video.value.link.indexOf("watch?v=") + 19
      );
      video.value.link = video.value.link.replace("watch?v=", "embed/");
    } else if (video.value.link.includes("youtu.be")) {
      video.value.id_youtube = video.value.link.substring(
        video.value.link.indexOf("youtu.be/") + 9,
        video.value.link.indexOf("youtu.be/") + 20
      );
      video.value.link = video.value.link.replace(
        "youtu.be/",
        "www.youtube.com/embed/"
      );
    } else if (video.value.link.includes("embed/")) {
      video.value.id_youtube = video.value.link.substring(
        video.value.link.indexOf("embed/") + 6,
        video.value.link.indexOf("embed/") + 17
      );
    }
  }
  if (video.value.key_words_mode != null) {
    video.value.key_words = video.value.key_words_mode.toString();
  }

  if (video.value.organization_id == null) {
    if (store.getters.user.organization_id == 1 && store.getters.user.IsSupper)
      video.value.organization_id = null;
    else video.value.organization_id = store.getters.user.organization_id;
  }
  if (files.length > 0) {
    var file = files[0];
    var fileReader = new FileReader();
    fileReader.onload = function () {
      var blob = new Blob([fileReader.result], { type: file.type });
      var url = URL.createObjectURL(blob);
      var vid = document.createElement("video");
      var timeupdate = function () {
        if (snapImage()) {
          vid.removeEventListener("timeupdate", timeupdate);
          vid.pause();
        }
      };
      vid.addEventListener("loadeddata", function () {
        if (snapImage()) {
          vid.removeEventListener("timeupdate", timeupdate);
        }
        video.value.duration = Math.round(this.duration);
        saveVideo_continue();
      });
      var snapImage = function () {
        var canvas = document.createElement("canvas");
        canvas.width = vid.videoWidth;
        canvas.height = vid.videoHeight;
        canvas
          .getContext("2d")
          .drawImage(vid, 0, 0, canvas.width, canvas.height);
        if (avts.length == 0) urlBase64 = canvas.toDataURL();
        let success = urlBase64.length > 100000;
        return success;
      };
      vid.addEventListener("timeupdate", timeupdate);
      vid.preload = "metadata";
      vid.src = url;
      // Load video in Safari / IE11
      vid.muted = true;
      vid.playsInline = true;
      vid.play();
    };
    fileReader.readAsArrayBuffer(file);
  } else {
    saveVideo_continue();
  }

  // var menuid = Object.keys(menu_IDNodeADD.value);
  // video.value.Menu_ID = menuid[0];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
};
watch(selectedVideos, () => {
  debugger;
  if (selectedVideos.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
//Sửa bản ghi
const editVideo = (datat) => {
  files = [];
  isHiddenFile.value = false;
  let data = JSON.parse(JSON.stringify(datat));
  submitted.value = false;

  displayBasic.value = true;
  if (data.key_words != null && data.key_words.length > 1) {
    if (!Array.isArray(data.key_words)) {
      data.key_words_mode = data.key_words.split(",");
    }
  }

  // menu_IDNodeADD.value[data.Menu_ID] = true;
  let contentFake = data.contents;
  data.contents = "";
  video.value = data;
  headerDialog.value = "Sửa video";
  isUpdateVideo.value = true;
  setTimeout(() => {
    video.value.contents = contentFake;
    isFirstVideo.value = true;
  }, "1500");
};
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const danhMucAdd = ref();
const openBasic = (str) => {
  files = [];
  avts = [];
  isDisplayAvt.value = false;
  urlBase64 = "";
  // loadRelate();
  video.value = {
    video_type: 0,
    status: 1,
    is_order: options.value.totalRecords + 1,
    is_comment: true,
    is_file_upload: true,
    organization_id: store.getters.user.organization_id,
    image: null,
    is_trimfile: false,
  };
  // menu_IDNodeADD.value = {};
  // menu_IDNodeADD.value[danhMucAdd.value[0].key] = true;
  files.value = [];
  submitted.value = false;
  headerDialog.value = str;
  isUpdateVideo.value = false;
  displayBasic.value = true;
};
const closeDialog = () => {
  isFirstVideo.value = false;
  //loadRelate();
  displayBasic.value = false;
};
const listMenu = ref();
const relateNew = ref([]);
const loadData = (rf) => {
  if (isDynamicSQL.value) {
    loadDataSQL();
    return false;
  }

  if (rf) {
    options.value.loading = true;
    loadCount();
  }
  // if (menu_ID.value == -1) {
  //   menu_ID.value = null;
  // }
  axios
    .post(
      baseURL + "/api/video_main/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "video_main_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "key_words", va: options.value.key_words },
              { par: "search", va: options.value.search },
              { par: "status", va: options.value.status },
              { par: "type_video", va: options.value.type_video },
              { par: "pageno", va: options.value.pageno || 1 },
              { par: "pagesize", va: options.value.pagesize || 20 },
            ],
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
        relateNew.value = [];
        data.forEach((element, i) => {
          element.is_sort =
            (options.value.pageno - 1) * options.value.pagesize + i + 1;
          if (element.start_date != null) {
            var date = element.start_date.split(" ");
            element.start_date = date[0];
          }
          if (element.end_date != null) {
            var date1 = element.end_date.split(" ");
            element.end_date = date1[0];
          }

          relateNew.value.push({
            name: element.title,
            code: element.video_id,
          });

          if (!element.created_date_show) element.created_date_show = null;
          element.created_date_show = moment(
            new Date(element.created_date)
          ).format("DD/MM/YYYY");
        });

        Array.from(new Set(relateNew.value));
        sttVideo.value = data.length + 1;
        datalists.value = data;
        console.log("Dữ liệu", relateNew.value);
      } else {
        datalists.value = [];
        console.log("Dữ liệu roong");
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi tải Video",
        controller: "Video.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

//Khai báo function
function saveVideo_continue() {
  var formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    // if file size 50MB -> trim
    if (file.size > 50 * (1024 * 1024)) {
      UploadFile(file);
      video.value.is_trimfile = true;
      formData.append("FileNameCovert", FileNameCovert);
    } else formData.append("video", file);
  }
  for (var i = 0; i < avts.length; i++) {
    let file = avts[i];
    formData.append("avatar", file);
  }
  formData.append("video", JSON.stringify(video.value));
  formData.append("urlBase64", urlBase64);
  if (!isUpdateVideo.value) {
    axios
      .post(baseURL + "/api/video_main/add_video", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm video thành công!");
          loadData(true);
          closeDialog();
        } else {
          swal.fire({
            title: "Thông báo!",
            text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
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
      });
  } else {
    axios
      .put(baseURL + "/api/video_main/update_video", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa video thành công!");
          loadData(true);
          closeDialog();
        } else {
          swal.fire({
            title: "Thông báo!",
            text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
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
      });
  }
}
const UploadFile = (TargetFile) => {
  // create array to store the buffer chunks
  var FileChunk = [];
  //trim video thanh phan nho voi size = 5mb
  var MaxFileSizeMB = 5;
  var BufferChunkSize = MaxFileSizeMB * (1024 * 1024);
  var FileStreamPos = 0;
  var EndPos = BufferChunkSize;
  var Size = TargetFile.size;
  while (FileStreamPos < Size) {
    FileChunk.push(TargetFile.slice(FileStreamPos, EndPos));
    FileStreamPos = EndPos; // jump by the amount read
    EndPos = FileStreamPos + BufferChunkSize; // set next chunk length
  }
  var TotalParts = FileChunk.length;
  var PartCount = 0;
  FileNameCovert = removeVietnameseTones(TargetFile.name);
  FileNameCovert = Math.floor(Math.random() * 1000) + FileNameCovert;
  FileChunk.forEach((item) => {
    PartCount++;
    var FilePartName = FileNameCovert + ".part_" + PartCount + "." + TotalParts;
    UploadFileChunk(item, FilePartName);
  });
};
const UploadFileChunk = (Chunk, FileName) => {
  let formData = new FormData();
  formData.append("video", Chunk, FileName);
  formData.append("dvid", store.getters.user.organization_id);
  axios({
    method: "post",
    url: baseURL + "/UploadLargeFile/UploadFile/",
    data: formData,
    config,
  });
};
const selectTree = () => {
  var menuid = Object.keys(menu_IDNode.value);
  menu_ID.value = menuid[0];
  loadData(true);
};
const filterTrangthai = ref();
const showFilter = ref(false);
// const filterVideo = () => {
//   filterSQL.value = [];
//   options.value.status = filterTrangthai.value;

//   options.value.pageno = 0;
//   showFilter.value = false;
//   loadData();
// };
//Tìm kiếm
const searchVideo = () => {
  options.value.pageno = 1;
  first.value = 1;
  loadData(true);
};

function removeVietnameseTones(alias) {
  var str = alias;
  if (!str) return "";
  /*str = str.toLowerCase();*/
  str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ |ặ|ẳ|ẵ/g, "a");
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
  str = str.replace(/đ/g, "d");

  str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
  str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
  str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
  str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
  str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
  str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
  str = str.replace(/Đ/g, "D");
  str = str.replace(
    /!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\:|\;|\'| |\"|\&|\#|\[|\]|~|$|_/g,
    "-"
  );
  // tìm và thay thế các kí tự đặc biệt trong chuỗi sang kí tự - /
  //str = str.replace(/-+-/g, "-"); //thay thế 2- thành 1-
  str = str.replace(/\-+$/g, "");
  //cắt bỏ ký tự - ở đầu và cuối chuỗi
  return str;
}
onMounted(() => {
  loadData(true);
  return {
    isFirst,
    options,
    danhMuc,
  };
});
</script>

<template>
  <div class="d-container">
    <div class="d-lang-header">
      <h3 class="d-module-title">
        <i class="pi pi-id-card"></i> Danh sách video ({{
          options.totalRecords
        }})
      </h3>
    </div>
    <Toolbar class="d-toolbar">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            v-model="options.search"
            @keyup.enter="searchVideo()"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
          />
          <Button
            :class="
              checkFilter ? 'ml-2' : 'ml-2 p-button-secondary p-button-outlined'
            "
            icon="pi pi-filter"
            @click="toggleFilter"
            aria-haspopup="true"
            aria-controls="overlay_panelS"
          />
          <OverlayPanel
            ref="filterButs"
            appendTo="body"
            :showCloseIcon="false"
            id="overlay_panelS"
            style="width: 350px"
            :breakpoints="{ '960px': '20vw' }"
          >
            <div class="grid formgrid m-2">
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4 p-0">Trạng thái:</div>
                <Dropdown
                  v-model="options.status"
                  :options="options_status"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Trạng thái"
                  class="col-8 p-0"
                  showClear="true"
                />
              </div>
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4 p-0">Loại video:</div>
                <Dropdown
                  v-model="options.type_video"
                  :options="options_type"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Loại video"
                  class="col-8 p-0"
                  showClear="true"
                />
              </div>
              <div class="col-12 field p-0">
                <Toolbar class="toolbar-filter">
                  <template #start>
                    <Button
                      @click="reFilterVideo"
                      class="p-button-outlined"
                      label="Xóa"
                    ></Button>
                  </template>
                  <template #end>
                    <Button @click="filterVideo" label="Lọc"></Button>
                  </template>
                </Toolbar>
              </div>
            </div>
          </OverlayPanel>
        </span>
        <!-- <TreeSelect
          style="margin-left: 24px; min-width: 200px"
          @change="selectTree()"
          v-model="menu_IDNode"
          :options="danhMuc"
          placeholder="Tất cả video"
        ></TreeSelect> -->
      </template>

      <template #end>
        <Button
          v-if="checkDelList"
          @click="deleteList()"
          label="Xóa"
          icon="pi pi-trash"
          class="mr-2 p-button-danger"
        />
        <Button
          @click="openBasic('Thêm mới')"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
        />
        <Button
          class="mr-2 p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          @click="onRefersh"
        />
      </template>
    </Toolbar>
    <div class="d-lang-table">
      <DataTable
        class="w-full p-datatable-sm e-sm"
        @nodeSelect="onNodeSelect"
        @nodeUnselect="onNodeUnselect"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        v-model:filters="filters"
        filterDisplay="menu"
        filterMode="lenient"
        dataKey="video_id"
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        :showGridlines="true"
        :rows="options.pagesize"
        :lazy="true"
        :value="datalists"
        :loading="options.loading"
        :paginator="true"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
        :totalRecords="options.totalRecords"
        :row-hover="true"
        v-model:selection="selectedVideos"
        v-model:first="first"
      >
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:50px;height:50px"
          bodyStyle="text-align:center;max-width:50px"
          selectionMode="multiple"
        >
        </Column>
        <Column
          :sortable="true"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:70px;height:50px"
          bodyStyle="text-align:center;max-width:70px"
          field="is_order"
          header="STT"
        >
          <template #body="data">
            <div
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            >
              {{ data.data.is_sort }}
            </div>
          </template>
        </Column>
        <Column
          headerStyle="text-align:left;height:50px"
          bodyStyle="text-align:left;"
          field="title"
          header="Tiêu đề"
          :sortable="true"
        >
          <template #body="data">
            <div
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            >
              {{ data.data.title }}
            </div>
          </template>
          <!-- <template #filter="{ filterModel }">
            <InputText
              type="text"
              v-model="filterModel.value"
              class="p-column-filter"
              placeholder="Tiêu đề video"
            />
          </template> -->
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px"
          field="image"
          header="Ảnh đại diện"
        >
          <template #body="data">
            <Image
              v-if="data.data.image"
              image-style="object-fit: cover; border: unset; outline: unset"
              width="100"
              height="50"
              alt=" "
              v-bind:src="
                data.data.image
                  ? basedomainURL + data.data.image
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              "
              preview
            />
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:160px;height:50px"
          bodyStyle="text-align:center;max-width:160px"
          field="created_date_show"
          header="Ngày tạo"
        >
          <template #body="data">
            <div
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            >
              {{ data.data.created_date_show }}
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:180px;height:50px"
          bodyStyle="text-align:center;max-width:180px"
          field="status"
          header="Trạng thái"
        >
          <template #body="data">
            <Dropdown
              @change="onDropDown(data.data)"
              class="col-11"
              v-model="data.data.status"
              :options="options_status"
              optionLabel="name"
              optionValue="code"
            >
              <template #value="slotProps">
                <div class="p-dropdown-car-value" v-if="slotProps.value">
                  <span v-if="slotProps.value == 1">Chưa duyệt</span>
                  <span v-if="slotProps.value == 2" style="color: #689f38"
                    >Đã duyệt</span
                  >
                  <span v-if="slotProps.value == 3" style="color: #fbc02d"
                    >Đã đóng</span
                  >
                  <span v-if="slotProps.value == 4" style="color: #d32f2f"
                    >Không duyệt</span
                  >
                </div>
                <span v-else>
                  {{ slotProps.placeholder }}
                </span>
              </template>
            </Dropdown>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px"
          header="Chức năng"
        >
          <template #body="data">
            <Button
              v-tooltip.top="'Chi tiết'"
              @click="openDetails(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-info-circle"
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            ></Button>
            <Button
              v-tooltip.top="'Sửa video'"
              @click="editVideo(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            ></Button>
            <Button
              v-tooltip.top="'Xóa video'"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
              @click="delVideo(data.data)"
            ></Button>
          </template>
        </Column>
        <template #empty>
          <div
            class="
              align-items-center
              justify-content-center
              p-4
              text-center
              m-auto
            "
            v-if="!isFirst"
          >
            <img src="../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
      </DataTable>
    </div>
  </div>

  <Dialog
    header="Chi tiết video"
    v-model:visible="displayDetails"
    :maximizable="true"
    :style="{ width: '60vw' }"
  >
    <form>
      <div v-if="video">
        <div class="col-12">
          <video
            v-if="video.is_file_upload"
            style="width: 100%"
            controls
            :src="basedomainURL + video.path"
          ></video>
          <YoutubeVue3
            v-if="!video.is_file_upload"
            ref="youtube"
            :autoplay="false"
            :videoid="video.id_youtube"
            controls="1"
            width="100%"
            height="100%"
            :style="'width: 100%; height: 34vw'"
          />
        </div>
        <div class="col-12 font-bold text-2xl">
          {{ video.title }}
        </div>
        <div class="col-12 flex">
          <div class="col-6 p-0">
            <span
              ><i class="pi pi-eye pr-1" style="font-size: 13px"></i>0 lượt
              xem</span
            >
          </div>
          <div class="col-6 p-0 flex justify-content-end">
            <span
              ><i class="pi pi-clock pr-1" style="font-size: 13px"></i>Ngày tải:
              {{
                moment(new Date(video.created_date)).format("HH:mm DD/MM/YYYY")
              }}</span
            >
          </div>
        </div>
        <div class="col-12 md:col-12">
          <hr />
        </div>
        <div class="col-12 flex pt-0 pb-3">
          <span style="color: #007ad4; font-weight: bold; font-size: 16px"
            ><i class="pi pi-comments pr-1" style="font-size: 16px"></i>Bình
            luận (0)</span
          >
        </div>
      </div>
    </form>
    <template #footer>
      <Button @click="closeDetails" label="Đóng" icon="pi pi-times" autofocus />
    </template>
  </Dialog>
  <Dialog
    @hide="closeDialog"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :maximizable="true"
    :style="{ width: '50vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 p-0 field flex">
          <div class="col-9 p-0">
            <div class="col-12 p-0 flex align-items-center field">
              <label class="text-left w-8rem"
                >Tiêu đề <span class="redsao">(*)</span></label
              >
              <InputText
                style="width: calc(100% - 8rem)"
                spellcheck="false"
                v-model="video.title"
              />
            </div>
            <div
              class="col-12 p-0 field flex"
              v-if="
                (v$.title.required.$invalid && submitted) ||
                v$.title.required.$pending.$response
              "
            >
              <div class="w-8rem"></div>
              <small style="width: calc(100% - 8rem)">
                <span style="color: red" class="w-full">{{
                  v$.title.required.$message
                    .replace("Value", "Tiêu đề")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div
              class="col-12 p-0 field flex"
              v-if="
                (v$.title.maxLength.$invalid && submitted) ||
                v$.title.maxLength.$pending.$response
              "
            >
              <div class="w-8rem"></div>
              <small style="width: calc(100% - 8rem)">
                <span style="color: red" class="w-full"
                  >{{
                    v$.title.maxLength.$message.replace(
                      "The maximum length allowed is",
                      "Tên đăng nhập không được vượt quá"
                    )
                  }}
                  ký tự</span
                >
              </small>
            </div>
            <div class="col-12 p-0 flex align-items-center field">
              <label class="text-left w-8rem">Từ khóa </label>
              <Chips
                style="width: calc(100% - 8rem)"
                v-model="video.key_words_mode"
                placeholder="Nhấn Enter để thêm"
              />
            </div>
            <div class="col-12 p-0 flex align-items-center field">
              <label class="text-left w-8rem">Video tải lên</label>
              <InputSwitch class="col-3" v-model="video.is_file_upload" />
            </div>
          </div>
          <div class="col-3 p-0">
            <div class="col-12 format-center pr-0 pb-0 relative">
              <img
                v-tooltip.top="'Ảnh đại diện'"
                @click="chonanh('AnhLang')"
                class="inputanh p-0"
                id="logoLang"
                v-bind:src="
                  video.image
                    ? basedomainURL + video.image
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />

              <Button
                v-if="video.image || isDisplayAvt"
                icon="pi pi-times"
                @click="delAvatar"
                class="p-button-rounded absolute top-0 right-0"
              />
            </div>
            <input
              class="ipnone"
              id="AnhLang"
              type="file"
              accept=".png,.jpg,.jpeg,.gif,.raw"
              @change="handleFileUpload"
            />
          </div>
        </div>
        <div class="flex field col-12 p-0" v-if="video.is_file_upload">
          <label class="text-left w-8rem"
            >File upload <span class="redsao">(*)</span></label
          >
          <div class="p-0" style="width: calc(100% - 8rem)">
            <FileUpload
              chooseLabel="Chọn File"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="false"
              :fileLimit="1"
              accept=".mp4"
              :maxFileSize="500000000"
              @select="onUploadFile"
              @remove="removeFile"
            />
          </div>
        </div>
        <div
          class="col-12 p-0 flex field"
          v-if="video.path && !isHiddenFile && video.is_file_upload"
        >
          <label class="w-8rem"></label>
          <div class="p-0 item-video" style="width: calc(100% - 8rem)">
            <Toolbar class="w-full py-3">
              <template #start>
                <div class="flex align-items-center">
                  <img
                    :src="basedomainURL + '/Portals/Image/mp4.png'"
                    style="object-fit: contain"
                    width="50"
                    height="50"
                  />
                  <span style="line-height: 50px"> {{ video.file_name }}</span>
                </div>
              </template>
              <template #end>
                <Button
                  icon="pi pi-times"
                  class="p-button-rounded p-button-danger"
                  @click="deleteFileCode(item)"
                />
              </template>
            </Toolbar>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex align-items-center"
          v-if="!video.is_file_upload"
        >
          <label class="w-8rem text-left"
            >Link video <span class="redsao">(*)</span></label
          >
          <InputText
            style="width: calc(100% - 8rem)"
            spellcheck="false"
            v-model="video.link"
          />
        </div>
        <div class="field col-12 flex p-0">
          <div class="col-4 p-0 align-items-center flex">
            <div class="w-8rem">Trạng thái</div>
            <Dropdown
              style="width: calc(100% - 8rem)"
              v-model="video.status"
              :options="options_status"
              optionLabel="name"
              optionValue="code"
              :disabled="!isUpdateVideo"
            />
          </div>
          <div class="col-1"></div>
          <div class="col-4 p-0 align-items-center flex">
            <div class="w-8rem pl-2">Bình luận</div>

            <InputSwitch class="col-3" v-model="video.is_comment" />
          </div>
          <div class="col-2 p-0 align-items-center flex">
            <div class="w-8rem text-left">STT</div>
            <InputText
              style="width: calc(100% - 0rem)"
              v-model="video.is_order"
              spellcheck="false"
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />

      <Button
        @click="saveVideo(!v$.$invalid)"
        label="Lưu"
        icon="pi pi-check"
        autofocus
      />
    </template>
  </Dialog>
</template>
<style scoped>
.d-container {
  background-color: #f5f5f5;
}

.d-lang-header {
  background-color: #ffff;
  padding: 12px 8px 0px 8px;
  margin: 8px 8px 0px 8px;
  height: 33px;
}
.d-lang-header h3,
i {
  font-weight: 600;
}
.d-module-title {
  margin: 0;
}
.d-lang-table {
  margin: 0px 8px 0px 8px;
  height: calc(100vh - 150px);
}

.d-toolbar {
  border: unset;
  outline: unset;
  background-color: #fff;
  margin: 0px 8px 0px 8px;
}

.d-btn-function {
  border-radius: 50%;
  margin-left: 6px;
}
.d-btn-delete {
  background-color: rgb(237, 114, 84);
  border: 1px solid rgb(214, 125, 125);
}
.d-btn-delete:hover {
  background-color: rgb(255, 0, 0);
  border: 1px solid rgb(214, 125, 125);
}
.d-btn-infor {
  background-color: rgb(56, 180, 187);
  border: 1px solid rgb(106, 173, 139);
}
.d-btn-infor:hover {
  background-color: rgb(125, 221, 150);
  border: 1px solid rgb(214, 125, 125);
}
.d-btn-edit:hover {
  background-color: rgb(63, 46, 252);
}
.inputanh {
  border: 1px solid #ccc;
  width: 100%;
  height: 120px;
  cursor: pointer;
  padding: 1px;
  object-fit: contain;
  background-color: #eeeeee;
}
.ipnone {
  display: none;
}

.d-avatar-video {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-video img {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  object-fit: contain;
}
.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
@keyframes p-progress-spinner-color {
  100%,
  0% {
    stroke: #858585 !important;
  }
  40% {
    stroke: #858585 !important;
  }
  66% {
    stroke: #858585 !important;
  }
  80%,
  90% {
    stroke: #858585 !important;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.item-video) {
  .p-toolbar-group-left {
    flex: 11;
  }
  .p-toolbar-group-right {
    flex: 1;
  }
}
</style>