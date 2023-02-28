<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";

import moment from "moment";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedNews = ref();
const checkDelList = ref(false);
const isFirstNews = ref(false);
const rules = {
  title: {
    required,
    $errors: [
      {
        $property: "title",
        $validator: "required",
        $message: "Tên tin tức không được để trống!",
      },
    ],
  },
  contents: {
    required,
    $errors: [
      {
        $property: "contents",
        $validator: "required",
        $message: "Nội dung tin tức không được để trống!",
      },
    ],
  },
};
const taskDateFilter = ref();
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
const menu_ID = ref();
const menu_IDNode = ref();
const menu_IDNodeADD = ref();

//Lọc theo ngày

const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};
const delDayClick = () => {
  taskDateFilter.value = [];
  options.value.start_date = null;
  options.value.end_date = null;
  loadData(true);
};
const onDayClick = () => {
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    options.value.start_date = taskDateFilter.value[0];
    options.value.end_date = taskDateFilter.value[1];
    if (!options.value.end_date)
      options.value.end_date = options.value.start_date;
    filterSQL.value = [];
    if (
      options.value.start_date &&
      options.value.start_date != options.value.end_date
    ) {
      let sDate = new Date(options.value.start_date);
      sDate.setDate(sDate.getDate() - 1);
      options.value.start_date = sDate;
      let filterS = {
        filterconstraints: [
          { value: options.value.start_date, matchMode: "dateAfter" },
        ],
        filteroperator: "and",
        key: "created_date",
      };
      filterSQL.value.push(filterS);
    }
    if (
      options.value.end_date &&
      options.value.start_date != options.value.end_date
    ) {
      let eDate = new Date(options.value.end_date);
      eDate.setDate(eDate.getDate() + 1);
      options.value.end_date = eDate;
      let filterS = {
        filterconstraints: [
          { value: options.value.end_date, matchMode: "dateBefore" },
        ],
        filteroperator: "and",
        key: "created_date",
      };
      filterSQL.value.push(filterS);
    }
    if (
      options.value.start_date &&
      options.value.start_date == options.value.end_date
    ) {
      let filterS1 = {
        filterconstraints: [
          { value: options.value.start_date, matchMode: "dateIs" },
        ],
        filteroperator: "and",
        key: "created_date",
      };
      filterSQL.value.push(filterS1);
    }
    loadDataSQL();
  }
};
//Xuất excel

//Xóa tin tức

const delNews = (News) => {
  swal
    .fire({
      title: "Thông báo",
      text:
        "Bạn có muốn xoá " +
        (News.news_type == 0 ? "tin tức" : "thông báo") +
        " này không!",
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
          .delete(baseURL + "/api/news_main/delete_news", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: News != null ? [News.news_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success(
                "Xoá " +
                  (News.news_type == 0 ? "tin tức" : "thông báo") +
                  " thành công!"
              );
              loadData(true);
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
const onPage = (event) => {
  options.value.pagesize = event.rows;
  options.value.pageno = event.page;

  loadDataSQL();
};
const filterSQL = ref([]);
const isDynamicSQL = ref(false);
const loadDataSQL = () => {
  let data = {
    id: "news_id",
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filternews_main", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];

      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
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
        controller: "News.vue",
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
  first.value = 0;
  options.value.pageno = 0;
  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData();
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField != "news_id") {
      options.value.sort +=
        ",news_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
    }

    isDynamicSQL.value = true;
    loadData();
  }
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

  options.value.pageno = 0;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadData(true);
};
//DropDown

const onDropDown = (value) => {
  let data = {
    IntID: value.news_id,
    TextID: value.news_id + "",
    IntTrangthai: value.status,
    BitTrangthai: false,
  };
  axios
    .put(baseURL + "/api/news_main/update_status", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Sửa trạng thái thành công!");
        loadData(false);
      } else {
        console.log("LỖI A:", response);
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
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.news_id,
    TextID: value.news_id + "",
    IntTrangthai: 0,
    BitTrangthai: value.is_hot,
    check: true,
  };
  axios
    .put(baseURL + "/api/news_main/update_ishot", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật tin thành công!");
        loadData(false);
      } else {
        console.log("LỖI A:", response);
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
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedNews.length);

  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa danh sách tin tức này không!",
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
        selectedNews.value.forEach((item) => {
          listId.push(item.news_id);
        });
        axios
          .delete(baseURL + "/api/news_main/delete_news", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá tin tức thành công!");
              checkDelList.value = false;

              loadData(true);
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const delAvatar = () => {
  news.value.image = null;
  files.value = [];
  document.getElementById("logoLang").onload = function () {
    URL.revokeObjectURL(document.getElementById("logoLang").src); // free memory
  };
  document.getElementById("logoLang").src =
    baseURL + "/Portals/Image/noimg.jpg";
};
//Lấy file ảnh
const chonanh = (id) => {
  document.getElementById(id).click();
};
const files = ref([]);
const handleFileUpload = (event) => {
  files.value = event.target.files;
 
  var output = document.getElementById("logoLang");
  if (event.target.files[0]) {
    output.src = URL.createObjectURL(event.target.files[0]);
    output.onload = function () {
      URL.revokeObjectURL(output.src); // free memory
    };
  } else {
    files.value = [];
    document.getElementById("logoLang").src =
      baseURL + "/Portals/Image/noimg.jpg";
  }
};
const toast = useToast();
const isFirst = ref(true);
const datalists = ref();
const isSaveNews = ref(false);
const sttNews = ref(1);
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
  sort: "news_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,

  loading: true,
  totalRecords: null,
});
const news = ref({
  is_order: 1,
  title: "",
  des: "",
  contents: "",
  image: "",
  is_hot: false,
  news_type: 0,
  key_words: "",
  IsWriter: "",
  start_date: "",
  end_date: null,
  status: false,
});
const v$ = useVuelidate(rules, news);
const loaiTinTuc = ref([
  { name: "Tin tức", code: 0 },
  { name: "Thông báo", code: 1 },
]);
const options_status = ref([
  { name: "Chưa duyệt", code: 1 },
  { name: "Đã duyệt", code: 2 },
  { name: "Đã đóng", code: 3 },
  { name: "Không duyệt", code: 4 },
]);
const danhMuc = ref();
//METHOD
const displayDetails = ref(false);
const openDetails = (data) => {
  displayDetails.value = true;
  news.value = data;
};
const closeDetails = () => {
  displayDetails.value = false;
  news.value = {};
};
const onUploadFileBug = (event) => {
  files.value = event.files;
};
const removeFileBug = (event) => {
  files.value = [];
};
const deleteFileCode = (value) => {
  news.value.url_file_save = news.value.url_file_save.filter((a) => a != value);
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "news_main_count",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "category_id", va: options.value.category_id },
          { par: "news_type", va: options.value.news_type },
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: options.value.status },
          { par: "is_hot", va: options.value.is_hot },
          { par: "is_notify", va: options.value.is_notify },

          { par: "start_date", va: options.value.start_date },
          { par: "end_date", va: options.value.end_date },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        sttNews.value = data[0].totalRecords + 1;
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
const resetRelate = () => {
  news.value.related_id_save = [];
};
const saveRelate = () => {
  document.getElementById("idsaveRelate").click();
};
const saveNews = (isFormValid) => {
  // document.getElementById("tinlienquan").click();
  submitted.value = true;
  if (!isFormValid) {
    return;
  }

  if (news.value.title.length >= 500) {
    swal.fire({
      title: "Thông báo",
      text:
        "Tên" +
        (news.value.news_type == 0 ? " tin tức" : " thông báo") +
        " không được vượt quá 500 kí tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (typeof news.value.start_date == "string") {
    var startDay = news.value.start_date.split("/");
    news.value.start_date = new Date(
      startDay[2] + "/" + startDay[1] + "/" + startDay[0]
    );
  }
  if (typeof news.value.end_date == "string") {
    var endDay = news.value.end_date.split("/");
    news.value.end_date = new Date(
      endDay[2] + "/" + endDay[1] + "/" + endDay[0]
    );
  }

  let formData = new FormData();
  for (var i = 0; i < files.value.length; i++) {
    let file = files.value[i];
    formData.append("image", file);
  }
  if (news.value.key_words_save != null) {
    news.value.key_words = news.value.key_words_save.toString();
  }

  if (news.value.related_id_save != null) {
    news.value.related_id = news.value.related_id_save.toString();
  }
  if (news.value.organization_id == null) {
    if (store.getters.user.organization_id == 1 && store.getters.user.IsSupper)
      news.value.organization_id = null;
    else news.value.organization_id = store.getters.user.organization_id;
  }
  if (
    Array.isArray(news.value.url_file_save) &&
    news.value.url_file_save != null
  ) {
    news.value.url_file = news.value.url_file_save.toString();
  }
   

  formData.append("news", JSON.stringify(news.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveNews.value) {
    axios
      .post(baseURL + "/api/news_main/add_news", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success(
            "Thêm " +
              (news.value.news_type == 0 ? "tin tức" : "thông báo") +
              " thành công!"
          );
          loadData(true);
          closeDialog();
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
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    axios
      .put(baseURL + "/api/news_main/update_news", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success(
            "Sửa " +
              (news.value.news_type == 0 ? "tin tức" : "thông báo") +
              " thành công!"
          );
          loadData(true);
          closeDialog();
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
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};
watch(selectedNews, () => {
  if (selectedNews.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const StartDateConvert = ref(new Date("1970/01/01"));
//Sửa bản ghi
const editNews = (data) => {
  submitted.value = false;

  displayBasic.value = true;
  files.value = [];
  if (data.key_words_save != null && data.key_words_save.length > 1) {
    if (!Array.isArray(data.key_words_save)) {
      data.key_words_save = data.key_words_save.split(",");
    }
  }
  if (data.related_id_save != null && data.related_id_save.length > 1) {
    if (!Array.isArray(data.related_id_save)) {
      data.related_id_save = data.related_id_save.split(",");
    }
  }
 
  data.start_date = new Date(data.start_date);
  if (data.end_date) data.end_date = new Date(data.end_date);
  let arrRelatedNotify = [];
  let arrRelateNews = [];
  relateNew.value.forEach((element) => {
    if (element.code != data.news_id) {
      arrRelateNews.push(element);
    }
  });
  relateNotify.value.forEach((element) => {
    if (element.code != data.news_id) {
      arrRelatedNotify.push(element);
    }
  });
  relateNotify.value = arrRelatedNotify;
  relateNew.value = arrRelateNews;
  // if (
  //   data.related_id_save != null &&
  //   data.related_id_save.length > 1 &&
  //   !Array.isArray(data.related_id_save)
  // ) {
  //   data.related_id_save = data.related_id_save.split(",");
  //   let arrRelate = [];
  //   relateNew.value.forEach((element) => {
  //     data.related_id_save.forEach((item) => {
  //       if (element.code == item) {
  //         arrRelate.push(element.code);
  //       }
  //     });
  //   });
  //   data.related_id_save = arrRelate;
  // }
  // menu_IDNodeADD.value = {};

  // menu_IDNodeADD.value[data.Menu_ID] = true;
  let contentFake = data.contents;
  data.contents = "";
  if (data.url_file_save != null && data.url_file_save != "")
    data.url_file_save = data.url_file_save.split(",");
  else data.url_file_save = null;
  news.value = data;
  headerDialog.value =
    news.value.news_type == 0 ? "Sửa tin tức" : "Sửa thông báo";

  isSaveNews.value = true;
  setTimeout(() => {
    news.value.contents = contentFake;
    isFirstNews.value = true;
  }, "1500");
};
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const danhMucAdd = ref();
const openBasic = (str) => {
  // loadRelate();
  news.value = {
    news_type: 0,
    status: 1,
    is_order: sttNews.value,
    is_hot: true,
    start_date: new Date(),
    is_comment: true,
  };

  // menu_IDNodeADD.value = {};
  // menu_IDNodeADD.value[danhMucAdd.value[0].key] = true;
  files.value = [];
  submitted.value = false;
  headerDialog.value = str;
  isSaveNews.value = false;
  displayBasic.value = true;
};
const closeDialog = () => {
  isFirstNews.value = false;
  loadData(false);
  displayBasic.value = false;
};
const listMenu = ref();
const relateNew = ref([]);
const relateNotify = ref([]);
const loadRelate = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "news_main_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "category_id", va: options.value.category_id },

          { par: "news_type", va: options.value.news_type },
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: options.value.status },
          { par: "is_hot", va: options.value.is_hot },
          { par: "is_notify", va: options.value.is_notify },
          { par: "datefilter", va: 0 },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
          { par: "start_date", va: options.value.start_date },
          { par: "end_date", va: options.value.end_date },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        relateNew.value = [];
        data.forEach((element) => {
          relateNew.value.push({
            name: element.title,
            code: element.news_id,
          });
        });
        Array.from(new Set(relateNew.value));
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi tải Tin tức liên quan",
        controller: "News.vue",
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
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "news_main_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "category_id", va: options.value.category_id },
          { par: "news_type", va: options.value.news_type },
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: options.value.status },
          { par: "is_hot", va: options.value.is_hot },
          { par: "is_notify", va: options.value.is_notify },
          { par: "datefilter", va: 0 },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
          { par: "start_date", va: options.value.start_date },
          { par: "end_date", va: options.value.end_date },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        relateNew.value = [];
        relateNotify.value = [];
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
          if (element.start_date != null) {
            var date = element.start_date.split(" ");
            element.start_date = date[0];
          }
          if (element.end_date != null) {
            var date1 = element.end_date.split(" ");
            element.end_date = date1[0];
          }
          if (element.news_type == 0)
            relateNew.value.push({
              name: element.title,
              code: element.news_id.toString(),
            });
          else {
            relateNotify.value.push({
              name: element.title,
              code: element.news_id.toString(),
            });
          }
          if (!element.url_file_save) element.url_file_save = element.url_file;
        
          if (!element.key_words_save)
            element.key_words_save = element.key_words;
          if (!element.created_date_show) element.created_date_show = null;
          if (!element.related_id_save)
            element.related_id_save = element.related_id;
          element.created_date_show = moment(
            new Date(element.created_date)
          ).format("DD/MM/YYYY");
        });
        Array.from(new Set(relateNew.value));
        Array.from(new Set(relateNotify.value));

        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
    })
    .catch((error) => {
     
      options.value.loading = false;
      addLog({
        title: "Lỗi tải Tin tức",
        controller: "News.vue",
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

const filterButs = ref();
const checkFilter = ref(false);
//Khai báo function
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const hideFilter = () => {
  if (
    !(
      options.value.is_hot != null ||
      options.value.status != null ||
      options.value.news_type != null
    )
  )
    checkFilter.value = false;
};
const selectTree = () => {
  var menuid = Object.keys(menu_IDNode.value);
  menu_ID.value = menuid[0];
  loadData(true);
};
const filterIsHot = ref();
const filterTrangthai = ref();
const filterNewsType = ref();
const showFilter = ref(false);
const reFilterNews = () => {
  checkFilter.value = false;
  filterIsHot.value = null;
  filterNewsType.value = null;
  filterTrangthai.value = null;
  options.value.is_hot = null;
  options.value.news_type = null;
  options.value.status = null;
  filterNews(false);
  showFilter.value = false;
};
const filterNews = (check) => {
  if (check) checkFilter.value = true;
  options.value.is_hot = filterIsHot.value;
  options.value.news_type = filterNewsType.value;
  options.value.status = filterTrangthai.value;
  if (options.value.is_hot != null) {
    let filterS = {
      filterconstraints: [{ value: options.value.is_hot, matchMode: "equals" }],
      filteroperator: "and",
      key: "is_hot",
    };
    filterSQL.value.push(filterS);
  }
  if (options.value.news_type != null) {
    let filterS = {
      filterconstraints: [
        { value: options.value.news_type, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "news_type",
    };
    filterSQL.value.push(filterS);
  }
  if (options.value.status != null) {
    let filterS = {
      filterconstraints: [{ value: options.value.status, matchMode: "equals" }],
      filteroperator: "and",
      key: "status",
    };
    filterSQL.value.push(filterS);
  }
  showFilter.value = false;

  loadDataSQL();
};
//Tìm kiếm
const searchNews = () => {
  loadDataSQL();
};
const first = ref(0);
const refreshData = () => {
  options.value.search = "";
  taskDateFilter.value = [];
  options.value.start_date = null;
  options.value.end_date = null;
  options.value.is_hot = null;
  options.value.status = null;
  options.value.news_type = null;
  filterIsHot.value = null;
  filterNewsType.value = null;
  filterTrangthai.value = null;
  filterSQL.value = [];
  selectedNews.value = [];
  filters.value = {
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
  };
  first.value = 0;
  options.value.pageno = 0;
  loadData(true);
};
onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
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
        <i class="pi pi-id-card"></i> Danh sách tin tức ({{
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
            @keyup.enter="searchNews()"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
          />
          <!-- :class="checkFilter?'':'p-button-secondary'" -->
          <Button
            :class="
              (filterIsHot != null ||
                filterNewsType != null ||
                filterTrangthai != null) &&
              checkFilter
                ? ''
                : 'p-button-secondary p-button-outlined'
            "
            class="ml-2"
            icon="pi pi-filter"
            @click="toggleFilter"
            aria-haspopup="true"
            aria-controls="overlay_panelS"
          />
          <OverlayPanel
            @hide="hideFilter"
            ref="filterButs"
            appendTo="body"
            :showCloseIcon="false"
            id="overlay_panelS"
            style="width: 350px"
            :breakpoints="{ '960px': '20vw' }"
          >
            <div class="grid formgrid m-2">
              <div class="field col-12 md:col-12">
                <div style="display: flex" class="col-12 p-0">
                  <div class="col-4 p-0 align-items-center flex">
                    Tin nổi bật:
                  </div>
                  <InputSwitch class="col-2 p-0" v-model="filterIsHot" />
                </div>
              </div>
              <div class="field col-12 md:col-12 flex">
                <div class="col-4 p-0 align-items-center flex">Phân loại:</div>
                <Dropdown
                  v-model="filterNewsType"
                  :options="loaiTinTuc"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Loại tin tức"
                  class="col-8 p-0"
                  :style="
                    filterNewsType != null ? 'border:2px solid #2196f3' : ''
                  "
                />
              </div>
              <div class="field col-12 md:col-12 flex">
                <div class="col-4 p-0 align-items-center flex">Trạng thái:</div>
                <Dropdown
                  v-model="filterTrangthai"
                  :options="options_status"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Trạng thái"
                  class="col-8 p-0"
                  :style="
                    filterTrangthai != null ? 'border:2px solid #2196f3' : ''
                  "
                />
              </div>

              <div class="col-12 field p-0">
                <Toolbar class="toolbar-filter">
                  <template #start>
                    <Button
                      @click="reFilterNews"
                      class="p-button-outlined"
                      label="Xóa"
                    ></Button>
                  </template>
                  <template #end>
                    <Button @click="filterNews(true)" label="Lọc"></Button>
                  </template>
                </Toolbar>
              </div>
            </div>
          </OverlayPanel>
        </span>
        <Calendar
          placeholder="Lọc theo ngày"
          id="range"
          v-model="taskDateFilter"
          :showIcon="true"
          selectionMode="range"
          class="mx-2"
          :manualInput="false"
        >
          <template #footer>
            <div class="w-full flex">
              <div class="w-4 format-center">
                <span @click="todayClick" class="cursor-pointer text-primary"
                  >Hôm nay</span
                >
              </div>
              <div class="w-4 format-center">
                <Button @click="onDayClick" label="Thực hiện"></Button>
              </div>
              <div class="w-4 format-center">
                <span @click="delDayClick" class="cursor-pointer text-primary"
                  >Xóa</span
                >
              </div>
            </div>
          </template>
        </Calendar>
        <!-- <TreeSelect
          style="margin-left: 24px; min-width: 200px"
          @change="selectTree()"
          v-model="menu_IDNode"
          :options="danhMuc"
          placeholder="Tất cả tin tức"
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
          @click="refreshData"
        />
      </template>
    </Toolbar>
    <div class="d-lang-table">
      <DataTable
        class="w-full p-datatable-sm e-sm"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        v-model:filters="filters"
        removableSort
        filterDisplay="menu"
        filterMode="lenient"
        dataKey="news_id"
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        :showGridlines="true"
        :rows="options.pagesize"
        :lazy="true"
        :value="datalists"
        :loading="options.loading"
        :paginator="true"
        :totalRecords="options.totalRecords"
        :row-hover="true"
        v-model:first="first"
        v-model:selection="selectedNews"
        :pageLinkSize="options.PageSize"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink    RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
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
              {{ data.data.is_order }}
            </div>
          </template>
        </Column>
        <Column
          headerStyle="text-align:left;height:50px"
          bodyStyle="text-align:left;"
          field="title"
          header="Tin tức"
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
          <template #filter="{ filterModel }">
            <InputText
              type="text"
              v-model="filterModel.value"
              class="p-column-filter"
              placeholder="Từ khoá"
            />
          </template>
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
              @error="
                $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
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
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px"
          field="is_hot"
          header="Tin mới"
          :sortable="true"
        >
          <template #body="data">
            <Checkbox
              :binary="data.data.is_hot"
              v-model="data.data.is_hot"
              @click="onCheckBox(data.data)"
            />
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px"
          field="news_type"
          header="Loại"
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
              {{ data.data.news_type == 0 ? "Tin tức" : "Thông báo" }}
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:180px;height:50px"
          bodyStyle="text-align:center;max-width:180px"
          field="status"
          header="Trạng thái"
          :sortable="true"
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
              v-tooltip.top="
                data.data.news_type == 0 ? 'Sửa tin tức' : 'Sửa thông báo'
              "
              @click="editNews(data.data)"
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
              v-tooltip.top="
                data.data.news_type == 0 ? 'Xóa tin tức' : 'Xóa thông báo'
              "
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
              @click="delNews(data.data)"
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
    header="Chi tiết tin tức"
    v-model:visible="displayDetails"
    :maximizable="true"
    :style="{ width: '60vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <h1>{{ news.title }}</h1>
        </div>
        <div class="field col-12 md:col-12">
          <img
            v-if="news.is_hot"
            style="width: 40px; height: 20px; margin-right: 12px"
            src="/src/assets/image/new.jpg"
            alt="new"
          />
          <span style="color: cornflowerblue; fon-size: 14px"
            >{{ news.created_name }},</span
          >
          <i style="padding: 0px 12px" class="pi pi-clock"></i
          ><span
            >Ngày:
            {{
              moment(new Date(news.start_date)).format("DD/MM/YYYY HH:mm:ss")
            }}</span
          >
        </div>
        <div class="field col-12 md:col-12">
          <hr />
        </div>
        <div class="field col-12 md:col-12">
          <h3>{{ news.des }}</h3>
        </div>
        <!-- <div
          v-if="news.image ? true : false"
          class="format-center field col-12 md:col-12"
        >
          <img v-bind:src="basedomainURL + news.image" />
        </div> -->
        <div
          style="padding: 0px 24px"
          class="field col-12 md:col-12 ck-content"
        >
          <p v-html="news.contents" style="font-size: 16px"></p>
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
    :style="{ width: '60vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="p-0" :class="news.news_type == 0 ? 'col-9' : 'col-12'">
            <div class="col-12 p-0 flex">
              <div class="w-8rem field text-left align-items-center flex">
                {{ news.news_type == 0 ? "Tên tin tức" : "Tên thông báo" }}
                <span class="redsao pl-1"> (*)</span>
              </div>
              <div style="width: calc(100% - 8rem)">
                <Textarea
                  v-model="news.title"
                  :autoResize="true"
                  rows="1"
                  cols="30"
                  spellcheck="false"
                  class="w-full"
                  id="txtr1"
                />
              </div>
            </div>
            <div class="col-12 p-0 field flex">
              <div class="w-8rem"></div>
              <small
                v-if="
                  (v$.title.$invalid && submitted) ||
                  v$.title.$pending.$response
                "
                style="width: calc(100% - 8rem)"
              >
                <span style="color: red" class="w-full">{{
                  v$.title.required.$message
                    .replace(
                      "Value",
                      news.news_type == 0 ? "Tên tin tức" : "Tên thông báo"
                    )
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div class="col-12 field p-0 flex">
              <div class="w-8rem text-left flex">
                <label>Mô tả</label>
              </div>
              <div style="width: calc(100% - 8rem)">
                <Textarea
                  v-model="news.des"
                  rows="5"
                  cols="30"
                  spellcheck="false"
                  class="w-full"
                  id="txtr2"
                />
              </div>
            </div>
          </div>
          <div class="col-3 p-0" v-if="news.news_type == 0">
            <!-- <label class="col-12 text-center p-0 format-center">Ảnh đại diện</label> -->

            <div class="col-12 format-center pr-0 pb-0 relative">
              <img
                v-tooltip.top="'Ảnh đại diện'"
                @click="chonanh('AnhLang')"
                class="inputanh p-0"
                id="logoLang"
                v-bind:src="
                  news.image
                    ? basedomainURL + news.image
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />

              <Button
                v-if="files.length > 0 || news.image"
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

        <div class="col-12 field relative">
          <div class="w-full flex text-left">
            <div class="w-8rem">Nội dung <span class="redsao">(*)</span></div>
            <div style="width: calc(100% - 8rem)">
              <ckeditor
                :editor="editor"
                :config="editorConfig"
                v-model="news.contents"
                :disabled="
                  isSaveNews && news.contents == '' && !isFirstNews
                    ? true
                    : false
                "
              ></ckeditor>
              <div
                style="translate: transform(-50%, -50%); top: 50%; right: 50%"
                v-if="isSaveNews && news.contents == '' && !isFirstNews"
                class="absolute"
              >
                <ProgressSpinner
                  style="width: 50px; height: 50px"
                  strokeWidth="8"
                  fill="var(--surface-ground)"
                  animationDuration=".5s"
                />
              </div>
            </div>
          </div>
        </div>

        <div class="field col-12 md:col-12 flex">
          <div class="w-8rem"></div>

          <small
            v-if="
              (v$.contents.$invalid && submitted) ||
              v$.contents.$pending.$response
            "
            style="width: calc(100% - 8rem)"
            class="col-10 p-error p-0"
          >
            <span style="color: red" class="col-12 p-0">{{
              v$.contents.required.$message
                .replace(
                  "Value",
                  news.news_type == 0
                    ? "Nội dung tin tức"
                    : "Nội dung thông báo"
                )
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-4 p-0 flex">
            <div class="w-8rem align-items-center flex">Ngày đăng</div>
            <div style="width: calc(100% - 8rem)">
              <Calendar
                :show-icon="true"
                class="w-full"
                id="basic"
                v-model="news.start_date"
                autocomplete="on"
                :show-time="true"
              />
            </div>
          </div>
          <div class="col-4 p-0 flex">
            <div class="w-8rem pl-2 align-items-center flex">Ngày hết hạn</div>
            <div style="width: calc(100% - 8rem)">
              <Calendar
                :show-icon="true"
                :minDate="news.start_date ? new Date(news.start_date) : false"
                class="w-full"
                id="basic"
                v-model="news.end_date"
                :show-time="true"
              />
            </div>

            <!-- <div class="col-4  align-items-center flex">
          
            <label
             
              class="col-4 text-left pl-0"
              >Bình luận</label
            >
            <InputSwitch
              class="col-2  pl-0"
              v-model="news.is_comment"
            />
          </div> -->
          </div>
          <div class="col-4 p-0 align-items-center flex">
            <div class="w-8rem px-2">Phân loại</div>
            <Dropdown
              style="width: calc(100% - 8rem)"
              v-model="news.news_type"
              :options="loaiTinTuc"
              optionLabel="name"
              optionValue="code"
              placeholder="Chọn loại tin"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex py-2">
          <div class="col-4 p-0 flex align-items-center flex">
            <div class="w-8rem text-left">STT</div>
            <InputText
              v-model="news.is_order"
              spellcheck="false"
              style="width: calc(100% - 8rem)"
            />
          </div>

          <div class="col-4 p-0 flex align-items-center">
            <div class="w-8rem pl-2 text-left">Trạng thái</div>
            <Dropdown
              style="width: calc(100% - 8rem)"
              v-model="news.status"
              :options="options_status"
              optionLabel="name"
              optionValue="code"
              :disabled="!isSaveNews"
            />
          </div>
          <!-- <div class="col-4 p-0 align-items-center flex">
            <div class="w-8rem pl-2">Chuyên mục</div>

            <TreeSelect
              style="width: calc(100% - 8rem)"
              v-model="menu_IDNodeADD"
              :options="danhMucAdd"
              placeholder="Chọn chuyên mục"
            ></TreeSelect>
          </div> -->
        </div>

        <div class="flex field col-12 md:col-12">
          <div class="col-4 p-0 flex">
            <div class="w-8rem text-left">Bình luận</div>
            <div style="width: calc(100% - 8rem)">
              <InputSwitch v-model="news.is_comment" />
            </div>
          </div>
          <div class="col-4 p-0 flex">
            <div class="w-8rem text-left px-2">Tin nổi bật</div>

            <div style="width: calc(100% - 8rem)">
              <InputSwitch v-model="news.is_hot" />
            </div>
          </div>
        </div>
        <div v-if="news.news_type == 1" class="field col-12 md:col-12 flex">
          <label class="w-8rem text-left align-items-center flex"
            >File đính kèm</label
          >
          <div style="width: calc(100% - 8rem)">
            <FileUpload
              chooseLabel="Chọn File"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="true"
              :maxFileSize="100000000"
              @select="onUploadFileBug"
              @remove="removeFileBug"
            >
            </FileUpload>
          </div>
        </div>

        <div class="col-12 field p-0 flex" v-if="news.url_file_save">
          <label class="w-8rem text-left align-items-center flex"></label>
          <div style="width: calc(100% - 8rem)">
            <div
              v-for="(item, index) in news.url_file_save"
              :key="index"
              class="flex"
            >
              <Toolbar class="w-full p-3 mx-2">
                <template #start>
                  <div class="flex">
                    <img
                      :src="
                        basedomainURL +
                        '/Portals/Image/file/' +
                        item.substring(item.indexOf('.') + 1) +
                        '.png'
                      "
                      style="object-fit: contain"
                      width="50"
                      height="50"
                      alt="logorar"
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/file/filess.png'
                      "
                    />
                    <span style="line-height: 50px" class="pl-2">
                      {{ item.substring(16) }}</span
                    >
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
        </div>

        <div class="field col-12 md:col-12 flex" v-if="news.news_type == 0">
          <label class="w-8rem text-left align-items-center flex"
            >Từ khóa</label
          >
          <Chips
            placeholder="Ấn Enter sau mỗi từ khóa"
            v-model="news.key_words_save"
            style="width: calc(100% - 8rem)"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label
            class="w-8rem text-left align-items-center flex"
            id="idsaveRelate"
            >Tin liên quan</label
          >
          <MultiSelect
            panelStyle="width:500px"
            panelClass="multi-width"
            :filter="true"
            v-model="news.related_id_save"
            :options="news.news_type == 1 ? relateNotify : relateNew"
            optionLabel="name"
            optionValue="code"
            placeholder="Tin liên quan"
            display="chip"
            style="width: calc(100% - 8rem)"
          >
            <template #footer="slotProps">
              <div>
                <Toolbar>
                  <template #end>
                    <div>
                      <Button
                        label="Lưu"
                        @click="saveRelate"
                        icon="pi pi-save"
                        class="mr-2 pr-2"
                      ></Button>
                      <Button
                        label="Xóa"
                        @click="resetRelate"
                        icon="pi pi-trash"
                        class="mr-2 p-button-danger"
                      ></Button>
                    </div>
                  </template>
                </Toolbar>
              </div>
            </template>
          </MultiSelect>
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
        @click="saveNews(!v$.$invalid)"
        label="Lưu"
        icon="pi pi-check"
        autofocus
      />
    </template>
  </Dialog>
</template>
<style scoped>
.ck-editor__editable {
  max-height: 500px !important;
}
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
  height: 156px;
  cursor: pointer;
  padding: 1px;
  object-fit: contain;
  background-color: #eeeeee;
}
.ipnone {
  display: none;
}

.d-avatar-news {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-news img {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  object-fit: contain;
}
.multi-width {
  max-width: 500px !important;
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