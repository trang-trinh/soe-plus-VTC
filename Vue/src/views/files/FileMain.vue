<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength, minLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import treeuser from "../../components/files/usertree.vue";
import foldershare from "../../components/files/sharefolder.vue";
import { onClickOutside } from "vue-click-outside-of";
import VueSimpleContextMenu from "vue-simple-context-menu";
import "vue-simple-context-menu/dist/vue-simple-context-menu.css";
import { useRouter, useRoute } from "vue-router";
import moment from "moment";
import { encr } from "../../util/function.js";
import { useCookies } from "vue3-cookies";
const { cookies } = useCookies();
// import "@lottiefiles/lottie-player";

const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const route = useRoute();
const router = inject("router");
const emitter = inject("emitter");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//Khai báo biến
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const list_modules = [
  {
    module_key: "M1",
    module_name: "Truyền thông",
    is_link: "/news/direct",
  },
  {
    module_key: "M2",
    module_name: "Lịch công tác",
    is_link: "calendarreserve",
  },
  {
    module_key: "M3",
    module_name: "Văn bản",
    is_link: "docreceive",
  },
  {
    module_key: "M4",
    module_name: "Công việc",
    is_link: "taskmain",
  },
  {
    module_key: "M5",
    module_name: "Luật",
    is_link: "lawmain",
  },
  {
    module_key: "M7",
    module_name: "Thiết bị",
    is_link: "doc_approved",
  },
  {
    module_key: "M8",
    module_name: "Trao đổi",
    is_link: "chat_message",
  },
];
var datalists_temp = [];
const max_length_file = ref(50);
const checkFilter = ref(false);
const headerDialogUser = ref("Chọn người dùng");
const ModalShare = ref(false);
var data_folder = [];
const log_files = ref([]);
const share_files = ref([]);
const typeTab = ref();
const active_delete = ref(false);
const isHiddenFile = ref(false);
const showSidebarInfo = ref(false);
const showInfoFileModule = ref(false);
const file_info = ref();
const displayDialogUser = ref(false);
const displayShareFolder = ref(false);
const itemBreads = ref([]);
const first = ref(0);
const vueSimpleContextMenu1 = ref();
const itemActive = ref();
const listItemClicked = ref([]);
const sttCate = ref(1);
const expandedKeys = ref({});
const selectCapcha = ref();
var data_tree = [];
const treefolders = ref();
const selectedKey = ref();
const datalists = ref([]);
const data_search = ref([]);
const is_search = ref(false);
const ListFolder = ref([]);
const isAdd = ref(true);
const folder = ref();
const filemain = ref();
const foldermain = ref();
const displayAddFile = ref(false);
const dataInfoNoti = ref();
const displayShowFileNoti = ref(false);
const displayAddFolder = ref(false);
const DataDetail = ref();
const ModalDetail = ref(false);
const images = ref([]);
const layout = ref("list");
const activeIndex = ref(0);
const displayAlbum = ref(false);
const isShowBtnDel = ref(false);
const file_edit = ref();
const module_top = ref();
const capacity_used = ref(0);
const capacity_total = ref(0);
const filterButs = ref();

const options = ref({
  SearchText: null,
  Status: null,
  loading: false,
  parent_id: null,
  pageno: 1,
  pagesize: 20,
  totalRecords: 0,
  MaxSTT: 0,
  search: null,
  condition: ">",
  typeUnit: "MB",
});
const units = ref([
  { name: "Bytes", value: "Bytes" },
  { name: "KB", value: "KB" },
  { name: "MB", value: "MB" },
  { name: "GB", value: "GB" },
]);
const loais = ref([
  { name: "Tài liệu", value: "file" },
  { name: "Kho dữ liệu", value: "folder" },
]);
const conditions = ref([
  { name: "Nhỏ hơn", value: "<" },
  { name: "Nhỏ hơn bằng", value: "<=" },
  { name: "Lớn hơn", value: ">" },
  { name: "Lớn hơn bằng", value: ">=" },
  { name: "Bằng", value: "=" },
]);
// refresh
const onRefresh = () => {
  options.value = {
    SearchText: null,
    Status: null,
    loading: false,
    parent_id: null,
    pageno: 1,
    pagesize: 20,
    totalRecords: 0,
    MaxSTT: 0,
    search: null,
  };
  layout.value = "list";
  itemSortButs.value.forEach((i) => {
    if (i.type_sort == "created_date_desc") {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  loadTudien(true);
};
const responsiveOptions = [
  {
    breakpoint: "1024px",
    numVisible: 5,
  },
  {
    breakpoint: "768px",
    numVisible: 3,
  },
  {
    breakpoint: "560px",
    numVisible: 1,
  },
];
const itemButMoreFile = ref([
  {
    label: "Tải xuống",
    icon: "pi pi-download",
    command: (event) => {
      openFile(filemain.value);
    },
  },
  {
    label: "Xóa",
    icon: "pi pi-trash",
    command: (event) => {
      deleteFileCode();
    },
  },
]);
const menuSortButs = ref();
const menuButMores = ref();
const menuButMoresFile = ref();
const itemButMores = ref([]);
const itemSortButs = ref([
  {
    label: "Ngày tạo mới đến cũ",
    type_sort: "created_date_desc",
    active: true,
    command: (event) => {
      ChangeSortFile(type_sort);
    },
  },
  {
    label: "Ngày tạo cũ đến mới",
    type_sort: "created_date_asc",
    active: false,
    command: (event) => {
      ChangeSortFile(type_sort);
    },
  },
  {
    label: "Kích thước tăng dần",
    type_sort: "capacity_asc",
    active: false,
    command: (event) => {
      ChangeSortFile(type_sort);
    },
  },
  {
    label: "Kích thước giảm dần",
    type_sort: "capacity_desc",
    ob: "ASC",
    active: false,
    command: (event) => {
      ChangeSortFile(type_sort);
    },
  },
  {
    label: "Tên từ A-Z",
    type_sort: "file_name_asc",
    active: false,
    command: (event) => {
      ChangeSortFile(type_sort);
    },
  },
  {
    label: "Tên từ Z-A",
    type_sort: "file_name_desc",
    active: false,
    command: (event) => {
      ChangeSortFile(type_sort);
    },
  },
]);

const optionsClick_List = ref([
  {
    label: "Sửa",
    icon: "pi pi-pencil",
    command: (event) => {
      file_edit.value.is_folder
        ? editFolder(file_edit.value)
        : editFile(file_edit.value);
    },
  },
  {
    label: "Thông tin",
    icon: "pi pi-info-circle mr-2",
    command: (event) => {
      showInfo(file_edit.value);
    },
  },
  {
    label: "Chia sẻ",
    icon: "pi pi-share-alt",
    command: (event) => {
      file_edit.value.is_folder
        ? showModalShareFolder(file_edit.value)
        : showModalShareFile(file_edit.value);
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      delFile(file_edit.value);
    },
  },
]);
const optionsShare_List = ref([
  {
    label: "Thông tin",
    icon: "pi pi-info-circle",
    command: (event) => {
      showInfo(file_edit.value);
    },
  },
  {
    label: "Gỡ chia sẻ",
    icon: "pi pi-replay",
    command: (event) => {
      delShare(file_edit.value);
    },
  },
]);

const optionsModule_List = ref([
  {
    label: "Thông tin",
    icon: "pi pi-info-circle",
    command: (event) => {
      showInfo_ModuleFiles(file_edit.value);
    },
  },
]);
const checkOverlay = (type) => {
  let check_share = [];
  if (type) {
    if (type.is_module) {
      check_share = [...optionsModule_List.value];
      if (!type.is_folder)
        check_share.push({
          label: "Chia sẻ",
          icon: "pi pi-share-alt",
          command: (event) => {
            file_edit.value.is_folder
              ? showModalShareFolder(file_edit.value)
              : showModalShareFile(file_edit.value);
          },
        });
    }
    // cua toi
    else if (type.is_share) {
      check_share = [...optionsShare_List.value];
      if (!type.is_folder && folder.value.is_read)
        check_share.push({
          label: "Tải xuống",
          icon: "pi pi-download",
          command: (event) => {
            openFile(file_edit.value);
          },
        }); // file -> cho download ve may
      // check quyen
      if (type.is_delete || folder.value.is_delete)
        check_share.push({
          label: "Xóa",
          icon: "pi pi-trash",
          command: (event) => {
            delFile(file_edit.value);
          },
        });
      if (type.is_edit || folder.value.is_edit)
        check_share.push({
          label: "Sửa",
          icon: "pi pi-pencil",
          command: (event) => {
            file_edit.value.is_folder
        ? editFolder(file_edit.value)
        : editFile(file_edit.value);
          },
        });
    }
    // chia se
    else check_share = optionsClick_List.value;
  }
  return check_share;
};
const toggleMores = (event, u) => {
  file_edit.value = u;
  itemButMores.value = checkOverlay(u);
  menuButMores.value.toggle(event);
};
const toggleFile = (event) => {
  menuButMoresFile.value.toggle(event);
};
const toggleSort = (event) => {
  menuSortButs.value.toggle(event);
};
const folder_tree = ref();

const submitted = ref(false);
const rules = {
  file_name: {
    required,
    maxLength: maxLength(250),
  },
};
const valis = {
  folder_name: {
    required,
    maxLength: maxLength(150),
  },
};
const v$ = useVuelidate(rules, filemain);
const t$ = useVuelidate(valis, foldermain);
//ADD log
const addLog = (log) => {
  axios.post(baseURL + "/api/FileMain/AddLog", log, config);
};
//watch
watch(selectCapcha, () => {
  if (selectCapcha) {
    let folder_id = Object.keys(selectCapcha.value)[0];
    if (folder_id) getCapacityFolder(folder_id);
    else capacity_total.value = -1;
  }
});
const getCapacityFolder = (id) => {
  axios
    .post(
      baseUrlCheck + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_folder_capacity_get",
            par: [{ par: "folder_id", va: id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        capacity_total.value = data[0][0].capacity;
        capacity_used.value = data[0][0].capacity_used;
      } else {
        capacity_total.value = -1; // khong gioi han
        capacity_used.value = 0;
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
//def
//get breadcumb
function getbreadcumb(id) {
  let itemBreads = [];
  let arr = data_tree.filter((x) => x.folder_id == id);
  if (id !== "me" && id !== "share" && arr.length > 0) {
    for (let i = 0; i < arr[0].path_id.split("/").length - 1; i++) {
      let obj = { folder_id: arr[0].path_id.split("/")[i] };
      let arr_filter = data_tree.filter(
        (x) => x.folder_id == arr[0].path_id.split("/")[i]
      );
      if (arr_filter.length > 0) {
        obj.label = arr_filter[0].folder_name;
        if (obj.label.length > 30)
          obj.label = obj.label.substring(0, 30) + "...";
        itemBreads.push(obj);
      }
    }
    //get id root breadcumb (home)
    let root_id = "share";
    if (!arr[0].is_share)
      root_id = data_tree.filter(
        (x) => x.folder_id == arr[0].path_id.split("/")[0]
      )[0].parent_id;
    let obj_root = data_tree
      .filter((x) => x.folder_id == root_id)
      .map((a) => ({ folder_id: a.folder_id, label: a.folder_name }));
    if (obj_root.length > 0) itemBreads.unshift(obj_root[0]);
  } else if (id == "me") {
    itemBreads.push({ folder_id: "me", label: "Của tôi" });
  } else if (id == "share") {
    itemBreads.push({ folder_id: "share", label: "Được chia sẻ" });
  }
  return itemBreads;
}
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const loadThumuc = (id, share, module_key) => {
  refilterFile();
  is_search.value = false;
  folder.value = data_tree.filter((x) => x.folder_id == id)[0];
  //load breadCumb
  itemBreads.value = [];
  //get bread
  itemBreads.value = getbreadcumb(id);
  itemActive.value = null;
  let expanted_coppy = JSON.parse(JSON.stringify(expandedKeys.value));
  expandedKeys.value = {};
  expandedKeys.value = expanted_coppy;
  //let folder_id_active = Object.keys(selectedKey.value)[0];
  expandedKeys.value[id] = true;
  selectedKey.value = {};
  selectedKey.value[id] = true;
  // swal.fire({
  //   width: 110,
  //   didOpen: () => {
  //     swal.showLoading();
  //   },
  // });
  if (id == "share") share = true;
  axios
    .post(
      baseUrlCheck + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_main_list1",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "folder_id", va: id },
              { par: "module_key", va: module_key || null }, // thu muc Khac
              { par: "share", va: share ? 1 : 0 }, //thu muc chia se
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      datalists.value = [];
      swal.close();
      let data = JSON.parse(response.data.data);
      //list folder
      //if (data[1].length && !share) {
      first.value = 0;
      if (data[1].length) {
        datalists.value = data[1];
      } else {
        datalists.value = [];
      }
      //list file
      if (data[0].length > 0) {
        datalists.value = datalists.value.concat(data[0]);
        options.value.totalRecords = data[0].length;
        images.value = data[0].filter((x) => x.is_image);
      } else {
        //datalists.value = data[1];
        options.value.totalRecords = 0;
      }
      datalists.value.forEach((item) => {
        if (item.is_folder)
          item.ratioMB =
            Math.floor(
              (item.capacity_used / (item.capacity * 1024 * 1024)) * 100
            ) || 0;
        item.capacityMB = formatBytes(
          item.is_folder ? item.capacity_used || 0 : item.capacity || 0
        );
        item.chon = false;
        item.labelContext =
          item.file_name +
          (item.created_name ? "\nNgười tạo: " + item.created_name : "") +
          (item.is_share && item.user_share
            ? "\nNgười chia sẻ: " + item.user_share
            : "") +
          "\n" +
          "Ngày sửa cuối: " +
          moment(new Date(item.modified_date || item.created_date)).format(
            "DD/MM/YYYY hh:mm"
          ) +
          (item.is_folder || isEmpty(item.file_type)
            ? ""
            : "\nType: " + item.file_type.toUpperCase() + " File") +
          (item.is_folder && item.capacityMB
            ? ""
            : "\nSize: " + item.capacityMB);
      });
      datalists_temp = JSON.parse(JSON.stringify(datalists.value));
      if (data[2].length > 0) {
        options.value.MaxSTT = data[2][0].MaxSTT;
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
const loadTudien = (f) => {
  ListFolder.value = [];
  axios
    .post(
      baseUrlCheck + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_main_folder_dictionary1",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        //loc theo quyen module user data
        data[0] = data[0].filter(
          (x) =>
            !x.module_key ||
            x.module_key == "M0" ||
            module_top.value.includes(x.module_key)
        );
        data_folder = data[0]
          .filter((x) => x.module_key == null)
          .map((x) => x.folder_id);
        let obj = renderTreeFolder(
          data[0].filter((x) => x.module_key == null),
          "folder_id",
          "folder_name"
        );
        treefolders.value = obj.arrtreeChils;
        let arr = [
          {
            folder_id: "me",
            folder_name: "Của tôi",
            parent_id: null,
            is_filepath: "/Portals/file/folder_me.png",
            count_folder: data[0].filter(
              (x) => x.module_key == null && !x.is_share && x.parent_id == null
            ).length,
            count_file:
              data[2].length > 0 ? data[2][0].count_root_file || 0 : 0,
          },
          {
            folder_id: "share",
            folder_name: "Được chia sẻ",
            parent_id: null,
            is_filepath: "/Portals/file/folder_share.png",
            count_folder: data[1].length,
            count_file:
              data[3].length > 0 ? data[3][0].count_share_file || 0 : 0,
          },
        ];
        data[0] = arr.concat(data[0]);
        if (data[1].length > 0) data[0] = data[0].concat(data[1]);
        //data breadcumb
        data_tree = data[0];
        // data[0].push({ folder_id: 0, folder_name: "Khác", parent_id: null });
        data[0].forEach((element) => {
          if (
            element.parent_id == null &&
            element.folder_id != "me" &&
            element.folder_id != "share" &&
            element.module_key == null
          )
            element.parent_id = "me";
          let taskcategory = {
            key: element.folder_id,
            data: element,
            name: element.folder_name,
            count_child: element.count_folder + element.count_file,
          };
          ListFolder.value.push(taskcategory);
        });
        RenderFolder(ListFolder);

        // active folder "me" - when firt load
        if (!isEmpty(route.params.id) && !isEmpty(route.params.type)) {
          loadThumuc("share");
          if (route.params.type == 1) loadThumuc(id);
          if (route.params.type == 2) {
            getInfoFileNoti(route.params.id);
          }
        } else if (f) loadThumuc("me");
      }
    });
};
const getInfoFileNoti = (id) => {
  axios
    .post(
      baseUrlCheck + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_info_get",
            par: [{ par: "file_id", va: id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        dataInfoNoti.value = data[0][0];
        displayShowFileNoti.value = true;
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const onNodeSelect = (node) => {
  //folder.value = node.data;
  loadThumuc(node.data.folder_id, node.data.is_share, node.data.module_key);
  // lưu log
  if (!["me", "share", "1"].includes(node.data.folder_id))
  debugger
    addLog({
      contents:
        store.getters.user.full_name +
        " đã xem thư mục " +
        (node.data.file_name || node.data.folder_name),
      folder_id: node.data.folder_id,
      log_type: 0,
      full_name: store.getters.user.full_name,
      name: node.data.file_name || node.data.folder_name,
    });
};
const showModalAddFile = () => {
  isAdd.value = true;
  submitted.value = false;
  selectCapcha.value = {};
  files.value = [];
  if (
    folder.value &&
    ((data_folder.length > 0 && data_folder.includes(folder.value.folder_id)) ||
      (folder.value.is_share && folder.value.is_edit)) //check quyen folder duoc chia se
  ) {
    selectCapcha.value[folder.value.folder_id] = true;
    if (folder.value.is_share && folder.value.is_edit)
      treefolders.value.push({
        key: folder.value.folder_id,
        data: folder.value.folder_id,
        label: folder.value.folder_name,
      });
  } else selectCapcha.value[-1] = true; // folder khac -> thu muc mac dinh la all
  filemain.value = {
    is_order: options.value.MaxSTT + 1,
    status: true,
    organization_id: store.getters.user.organization_id,
    is_public: true,
  };
  displayAddFile.value = true;
};
const SaveFile = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (files.value.length == 0 && isAdd.value) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn file upload!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (
    isAdd.value &&
    files.value[0].size > max_length_file.value * 1024 * 1024
  ) {
    swal.fire({
      title: "Thông báo!",
      text:
        "Vui lòng upload file có kích thước nhỏ hơn " +
        max_length_file.value +
        "MB!",
      icon: "error",
      confirmButtonText: "OK",
    });
    files.value = [];
    return;
  }
  if (selectCapcha.value) {
    filemain.value.folder_id = Object.keys(selectCapcha.value)[0];
    if (filemain.value.folder_id == "-1") filemain.value.folder_id = null;
  }
  if (filemain.value.keywords_mode)
    filemain.value.keywords = filemain.value.keywords_mode.toString();
  if (filemain.value.folder_id)
    filemain.value.path_folder = getbreadcumb(filemain.value.folder_id)
      .map((x) => x.label)
      .join("/");
  else filemain.value.path_folder = "Của tôi";
  let formData = new FormData();
  if (files.value.length > 0) {
    for (var i = 0; i < files.value.length; i++) {
      let file = files.value[i];
      filemain.value.capacity = file.size;
      filemain.value.file_type = file.name.substr(
        file.name.lastIndexOf(".") + 1
      );
      formData.append("file", file);
    }
  }
  if (
    capacity_total.value != -1 &&
    capacity_used.value + filemain.value.capacity >
      capacity_total.value * 1024 * 1024
  ) {
    swal.fire({
      title: "Thông báo!",
      text: "Dung lượng thư mục đã đầy, hãy thử upload file có kích thước nhỏ hơn!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  formData.append("model", JSON.stringify(filemain.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/FileMain/${
        isAdd.value == false ? "Update_FileMain" : "Add_FileMain"
      }`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        isAdd.value == false
          ? toast.success("Cập nhật tài liệu thành công!")
          : toast.success("Thêm tài liệu thành công!");
        loadTudien();
        loadThumuc(filemain.value.folder_id || "me");
        displayAddFile.value = false;
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
};
const saveFolder = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (selectCapcha.value) {
    let keys = Object.keys(selectCapcha.value);
    foldermain.value.parent_id = keys[0];
    if (foldermain.value.parent_id == "-1") foldermain.value.parent_id = null;
  }
  if (foldermain.value.keywords)
    foldermain.value.keywords = foldermain.value.keywords.toString();
  let formData = new FormData();
  formData.append("model", JSON.stringify(foldermain.value));
  formData.append("folder_share", JSON.stringify([]));
  // fake label file name or folder name
  file_edit.value.file_name = foldermain.value.folder_name;
  axios({
    method: "put",
    url: baseURL + `/api/FileFolder/${"Update_Folder"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        let id = response.data.data;
        swal.close();
        isAdd.value == false
          ? toast.success("Cập nhật thư mục thành công!")
          : toast.success("Thêm thư mục thành công!");
        displayAddFolder.value = false;
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
      console.log(error);
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
//file upload
const removeUploadedFile = (event) => {};
const files = ref([]);
const uploadFile = ref(null);
const onUploadFile = (event) => {
  if (!isAdd.value) {
    isHiddenFile.value = true;
  }
  event.files.forEach((fi, index) => {
    let formData = new FormData();
    formData.append("fileupload", fi);
    axios({
      method: "post",
      url: baseUrlCheck + `/api/FileMain/ScanFileUpload`,
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          if (fi.size > max_length_file.value * 1024 * 1024) {
            swal.fire({
              title: "Thông báo!",
              text:
                "Vui lòng upload file có kích thước nhỏ hơn " +
                max_length_file.value +
                "MB!",
              icon: "error",
              confirmButtonText: "OK",
            });
            uploadFile.value.clear();
            files.value = [];
          }
          // accept
          else files.value[index] = fi;
        } else {
          uploadFile.value.clear();
          files.value = [];
          swal.fire({
            title: "Cảnh báo",
            text: "File bị xóa do tồn tại mối đe dọa với hệ thống!",
            icon: "warning",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  });
};
const removeFile = (event) => {
  files.value = files.value.filter((a) => a != event.file);
};
const deleteFileCode = (value) => {
  filemain.value.is_filepath = null;
  files.value = [];
};
const clickDelFile = () => {
  if (
    datalists.value.filter((x) => x.chon == true).length > 0 ||
    data_search.value.filter((x) => x.chon == true).length > 0
  ) {
    isShowBtnDel.value = true;
  } else isShowBtnDel.value = false;
};
const openFile = (file) => {
  // add log
  addLog({
    contents:
      store.getters.user.full_name + " đã tải xuống tài liệu " + file.file_name,
    file_id: file.file_id,
    log_type: 1,
    full_name: store.getters.user.full_name,
    name: file.file_name,
  });
  var url = baseURL + file.is_filepath;
  var name = file.name_file || "file_download";
  const a = document.createElement("a");
  a.href =
    basedomainURL +
    "/Viewer/DownloadFile?url=" +
    file.is_filepath +
    "&title=" +
    name;
  a.download = name;
  a.target = "_blank";
  a.click();
  a.remove();
};
//bỏ chia sẻ
const delShare = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text:
        "Bạn có muốn bỏ chia sẻ " +
        (md.is_folder ? "thư mục" : "tài liệu") +
        " này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      let data = { id: [md.file_id], user_id: store.getters.user.user_id };
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        axios
          .delete(
            baseURL +
              "/api/" +
              (md.is_folder ? "FileFolder/" : "FileMain/") +
              (md.is_folder ? "Del_SharePublic" : "Del_Share"),
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: data,
            }
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Gỡ chia sẻ thành công!");
              loadTudien();
              loadThumuc(folder.value.folder_id, folder.value.is_share);
              // if (!md) selectedNodes.value = [];
            } else {
              swal.fire({
                title: "Thông báo!",
                text: "Gỡ chia sẻ không thành công, vui lòng thử lại",
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
// xóa 1
const delFile = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá tài liệu này không!",
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
          .delete(
            baseURL +
              "/api/" +
              (md.is_folder ? "FileFolder/Del_Folder" : "FileMain/Del_Files"),
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: md != null ? [md.file_id] : selectedNodes.value,
            }
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              md.is_folder
                ? toast.success("Xoá thư mục thành công!")
                : toast.success("Xoá tài liệu thành công!");
              loadTudien();
              loadThumuc(folder.value.folder_id, folder.value.is_share);
              // if (!md) selectedNodes.value = [];
            } else {
              swal.fire({
                title: "Thông báo!",
                text: "Xóa không thành công, vui lòng thử lại",
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
// xóa nhiều
const delListFiles = (listItem) => {
  if (layout.value == "list") listItem = datalists.value.filter((x) => x.chon);
  let is_check_folder = false;
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa tài liệu này không!",
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
        (async () => {
          await axios
            .delete(baseURL + "/api/FileFolder/Del_Folder", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listItem.filter((x) => x.is_folder).map((a) => a.file_id),
            })
            .then((response) => {
              if (response.data.err != "1") {
                is_check_folder = true;
                // let idx = ListFolder.value.filter((x) => x.key == "78FD5A0509564A2D863A0E16AEDF60ED");
                // ListFolder.value.splice(idex,1);
                loadTudien();
                // removeFolderInTree(listItem.filter((x) => x.is_folder).map((a) => a.file_id));
              } else is_check_folder = true;
            })
            .catch((error) => {
              if (error.status === 401) {
                swal.fire({
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  confirmButtonText: "OK",
                });
              }
            });
          await axios
            .delete(baseURL + "/api/FileMain/Del_Files", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listItem.filter((x) => !x.is_folder).map((a) => a.file_id),
            })
            .then((response) => {
              if (response.data.err != "1") {
                is_check_folder = true;
              } else {
                is_check_folder = false;
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
          swal.close();
          if (is_check_folder) {
            toast.success("Xoá tài liệu thành công!");
            isShowBtnDel.value = false;
            loadTudien();
            loadThumuc(folder.value.folder_id, folder.value.is_share);
          } else {
            swal.fire({
              title: "Thông báo!",
              text: "Xóa không thành công, vui lòng thử lại",
              icon: "error",
              confirmButtonText: "OK",
            });
          }
        })();
      }
    });
};
const editFile = (file) => {
  files.value = [];
  isAdd.value = false;
  isHiddenFile.value = false;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseUrlCheck + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_info_get",
            par: [{ par: "file_id", va: file.file_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        filemain.value = data[0][0];
        selectCapcha.value = {};
        selectCapcha.value[filemain.value.folder_id || "-1"] = true;
        if (filemain.value.keywords != null) {
          if (!Array.isArray(filemain.value.keywords)) {
            filemain.value.keywords_mode = filemain.value.keywords.split(",");
          }
        }
      }
      displayAddFile.value = true;
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const editFolder = (folder) => {
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseUrlCheck + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_folder_get",
            par: [{ par: "folder_id", va: folder.folder_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        foldermain.value = data[0][0];
        if (foldermain.value.keywords !== null)
          foldermain.value.keywords = foldermain.value.keywords.split(",");
        selectCapcha.value = {};
        selectCapcha.value[foldermain.value.parent_id || "-1"] = true;
      }
      displayAddFolder.value = true;
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
//def
const renderTreeFolder = (data, id, name) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
      //
      om = { key: m[id], data: m[id], label: m[name] };
      const retreechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em[id], label: em[name] };
            retreechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      retreechildren(om, m[id]);
      arrtreeChils.push(om);
    });
  arrtreeChils.unshift({
    key: -1,
    data: -1,
    label: "Chọn thư mục",
  });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    // for (let child of node.children) {
    //   expandNode(child);
    // }
  }
};
const RenderFolder = (ListFolder) => {
  let List = [];
  ListFolder.value
    .filter((c) => c.data.parent_id == null)
    .forEach((element, i) => {
      sttCate.value = sttCate.value + i + 1;

      let Cat = {
        key: element.data.folder_id,
        data: element.data,
        name: element.data.folder_name,
        count_child: element.data.count_folder + element.data.count_file,
      };

      const RenderChild = (child, folder_id) => {
        if (!child.children) child.children = [];
        let listChilCate = ListFolder.value.filter(
          (c) => c.data.parent_id == folder_id
        );
        listChilCate.forEach((element) => {
          let CatChild = {
            key: element.data.folder_id,
            data: element.data,
            name: element.data.folder_name,
            count_child: element.data.count_folder + element.data.count_file,
          };
          if (!CatChild.children) CatChild.children = [];
          RenderChild(CatChild, element.data.folder_id);
          child.children.push(CatChild);
        });
      };
      RenderChild(Cat, element.data.folder_id);
      List.push(Cat);
    });
  folder_tree.value = List;
};
const dbFileClick = (data) => {
  ModalDetail.value = true;
  // check quyen
  if (folder.value && folder.value.is_share && !folder.value.is_read) {
    DataDetail.value = null;
  } else {
    DataDetail.value = data;
    addLog({
      contents:
        store.getters.user.full_name + " đã xem tài liệu " + data.file_name,
      file_id: data.file_id,
      log_type: 0,
      full_name: store.getters.user.full_name,
      name: data.file_name,
    });
  }
};
const dbImageClick = (fi) => {
  if (folder.value && folder.value.is_share && !folder.value.is_read) {
    DataDetail.value = null;
  } else {
    let idx = images.value.findIndex((x) => x.file_id == fi.file_id);
    activeIndex.value = idx;
    displayAlbum.value = true;
    addLog({
      contents:
        store.getters.user.full_name + " đã xem tài liệu " + fi.file_name,
      file_id: fi.file_id,
      log_type: 0,
      full_name: store.getters.user.full_name,
      name: fi.file_name,
    });
  }
};
const formatBytes = (bytes, decimals = 2) => {
  if (bytes === 0) return "0 Bytes";

  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};
const goPrevious = (data) => {
  folder.value = data_tree.filter((x) => x.folder_id == data.parent_id)[0];
  loadThumuc(
    folder.value.folder_id,
    folder.value.is_share,
    folder.value.module_key
  );
};
function useBoxForClickOutside(target) {
  onClickOutside((mousedownEv, mouseupEv) => {
    if (
      ![
        "pi pi-trash p-button-icon p-button-icon-left",
        "p-button p-component mr-2 p-button-danger btn-delete",
        "p-button-label",
        "button.swal2-cancel.swal2-styled.swal2-default-outline",
        "swal2-cancel swal2-styled swal2-default-outline",
      ].includes(mouseupEv.srcElement.className)
    ) {
      itemActive.value = null;
      listItemClicked.value = [];
    }
    // check button delete
    if (listItemClicked.value.length > 0) active_delete.value = true;
    else active_delete.value = false;
  }, target);
  return {};
}
const optionsClick_Grid = [
  {
    name: "<i class='pi pi-pencil mr-2'></i>Sửa",
    type: 1,
    class: "",
  },
  {
    type: "divider",
  },
  {
    name: "<i class='pi pi-info-circle mr-2'></i>Thông tin",
    type: 4,
  },
  {
    name: "<i class='pi pi-share-alt mr-2'></i>Chia sẻ",
    type: 2,
  },
  {
    name: "<i class='pi pi-trash mr-2'></i>Xóa",
    type: 3,
  },
];
const optionsShare = [
  {
    name: "<i class='pi pi-info-circle mr-2'></i>Thông tin",
    type: 4,
  },
  {
    name: "<i class='pi pi-replay mr-2'></i>Gỡ chia sẻ",
    type: 5,
  },
];
const optionsModule_Grid = [
  {
    name: "<i class='pi pi-info-circle mr-2'></i>Thông tin",
    type: 6,
  },
];
const checkoption = (data) => {
  let check_share = [];
  if (data) {
    if (data.is_module) {
      check_share = [...optionsModule_Grid];
      if (!data.is_folder)
        check_share.push({
          name: "<i class='pi pi-share-alt mr-2'></i>Chia sẻ",
          type: 2,
        });
    }
    // cua toi
    else if (data.is_share) {
      check_share = [...optionsShare];
      if (!data.is_folder && folder.value.is_read)
        check_share.push({
          name: "<i class='pi pi-download mr-2'></i>Tải xuống",
          type: 7,
        }); // file -> cho download ve may
      // check quyen
      if (data.is_delete || folder.value.is_delete)
        check_share.push({
          name: "<i class='pi pi-trash mr-2'></i>Xóa",
          type: 3,
        });
      if (data.is_edit || folder.value.is_edit)
        check_share.unshift({
          name: "<i class='pi pi-pencil mr-2'></i>Sửa",
          type: 1,
        });
    }
    // chia se
    else check_share = optionsClick_Grid;
  }
  return check_share;
};
const optionClicked = (event) => {
  file_edit.value = event.item;
  switch (event.option.type) {
    case 1:
      if (file_edit.value.is_folder) {
        editFolder(file_edit.value);
      } else editFile(file_edit.value);
      break;
    case 2:
      if (file_edit.value.is_folder) {
        showModalShareFolder(file_edit.value);
      } else showModalShareFile(file_edit.value);

      break;
    case 3:
      delFile(file_edit.value);
      break;
    case 4:
      showInfo(file_edit.value);
      break;
    case 5:
      delShare(file_edit.value);
      break;
    case 6:
      showInfo_ModuleFiles(file_edit.value);
      break;
    case 7:
      openFile(file_edit.value);
      break;
    default:
      break;
  }
};
// function handleClick1(event, item) {
//   item_clicked.value = item.is_share;
//   this.vueSimpleContextMenu1.showMenu(event, item);
// }
const target = ref(null);
const { onClick } = useBoxForClickOutside(target);
const clickItemFile = (item) => {
  // key ctrl
  if (pressedKeys.value["17"]) {
    let idx = -1;
    if (listItemClicked.value.length > 0)
      idx = listItemClicked.value.findIndex((x) => x.file_id == item.file_id);
    if (idx == -1) {
      listItemClicked.value.push(item);
    } else listItemClicked.value.splice(idx, 1);
  } else listItemClicked.value = [item];
  // check button delete
  // item.is_share- chia se; is_module- tu module he thong
  if (listItemClicked.value.length > 0 && !item.is_share && !item.is_module)
    active_delete.value = true;
  else active_delete.value = false;
};
const item_grid = ref("grid-8");
const resize_ob = new ResizeObserver(function (entries) {
  let width = entries[0].contentRect.width;
  // defaul width size item with my sreen width = 1600px
  let item_width = 124;
  //  sreen width ratio item_width
  if (screen.width) item_width = item_width * (screen.width / 1600);
  if (width >= item_width * 9) item_grid.value = "grid-9";
  else if (width >= item_width * 8) item_grid.value = "grid-8";
  else if (width >= item_width * 7) item_grid.value = "grid-7";
  else if (width >= item_width * 6) item_grid.value = "grid-6";
  else if (width >= item_width * 5) item_grid.value = "grid-5";
  else if (width >= item_width * 4) item_grid.value = "grid-4";
  else if (width >= item_width * 3) item_grid.value = "grid-3";
  else item_grid.value = "grid-2";
});
//share file
const showModalShareFile = (item) => {
  file_edit.value = item;
  //ModalShare.value =true;
  displayDialogUser.value = true;
  // init phong ban
  //initOrganization();
  //initUser();
};
const showModalShareFolder = (item) => {
  displayShareFolder.value = true;
};
const showInfo_ModuleFiles = (item) => {
  showInfoFileModule.value = true;
  file_info.value = item;
  file_info.value.count_size_MB = formatBytes(file_info.value.count_size);
};
const showInfo = (item) => {
  file_info.value = {}; //clear data
  typeTab.value = 1;
  showSidebarInfo.value = true;
  file_info.value = item;
  if (file_info.value.capacity_used) {
    file_info.value.tyle = Math.round(
      (file_info.value.capacity_used /
        (file_info.value.capacity * 1024 * 1024)) *
        100
    );
    if (file_info.value.tyle > 100) file_info.value.tyle = 100;
  } else {
    file_info.value.tyle = 0;
    file_info.value.capacity_used = 0;
  }
  file_info.value.tyle_text = file_info.value.tyle + "%";
  if (file_info.value.tyle < 70) {
    file_info.value.cssprogressbar = "bg-success-folder";
  } else if (file_info.value.tyle >= 70 && file_info.value.tyle < 90) {
    file_info.value.cssprogressbar = "bg-info-folder";
  } else if (file_info.value.tyle >= 90)
    file_info.value.cssprogressbar = "bg-warning-folder";
  file_info.value.capacity_used_MB = formatBytes(file_info.value.capacity_used);
  //get info
  axios
    .post(
      baseUrlCheck + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_get_info",
            par: [
              { par: "id", va: item.file_id.toString() },
              { par: "type", va: item.is_folder ? 0 : 1 },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = JSON.parse(response.data.data);
      log_files.value = [];
      share_files.value = [];
      if (data[0].length > 0) {
        log_files.value = data[0];
      }
      if (data[1].length > 0) {
        share_files.value = data[1];
      }
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        confirmButtonText: "OK",
      });
      return;
    });
};
// start observing for resize
const saveShareFile = (data, type) => {
  let formData = new FormData();
  if (!type) formData.append("file_id", file_edit.value.file_id);
  else {
    formData.append("file_model", JSON.stringify(file_edit.value));
  }
  formData.append("model", JSON.stringify(data));
  axios({
    method: "put",
    url: baseURL + `/api/FileMain/${type ? "Share_File_Module" : "Share_File"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        let id = response.data.data;
        swal.close();
        toast.success("Chia sẻ thành công!");
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
      console.log(error);
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const saveShareFolder = (data) => {
  let formData = new FormData();
  formData.append(
    "model",
    JSON.stringify({
      folder_id: file_edit.value.file_id,
      type_share: file_edit.value.type_share,
    })
  );
  formData.append("folder_share", JSON.stringify(data));
  axios({
    method: "put",
    url: baseURL + `/api/FileMain/${"Share_Folder"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        let id = response.data.data;
        swal.close();
        toast.success("Cập nhật thành công!");
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
      console.log(error);
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
// const clearSearch = () => {
//   is_search.value = false;
//   options.value.search = null;
// };
const onSearch = () => {
  //let temp = JSON.parse(JSON.stringify(datalists.value))
  datalists.value = datalists_temp;
  if (options.value.search) {
    is_search.value = true;
    FilterAndSearch();
  } else {
    is_search.value = false;
    // data_search.value = [];
  }
};
const goToModule = (data) => {
  let mds = list_modules.filter((x) => data.module_key.includes(x.module_key));
  if (mds.length > 0) {
    router.push({ name: mds[0].is_link, params: {} });
  }
};
const initModulesMenu = () => {
  axios
    .post(
      baseUrlCheck + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_listmodulestop",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        var new_arr = [
          { module_key: "M1.1" },
          { module_key: "M1.2" },
          { module_key: "M1.3" },
          { module_key: "M3.1" },
          { module_key: "M3.2" },
          { module_key: "M3.3" },
        ];
        module_top.value = data[0].concat(new_arr).map((x) => x.module_key);
      }
    })
    .then((response) => {
      loadTudien(true);
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
//lọc

function FilterAndSearch() {
  if (options.value.search)
    datalists.value = datalists.value.filter((x) =>
      removeVietnameseTones(x.file_name.toLowerCase()).includes(removeVietnameseTones(options.value.search.toLowerCase()))
    );
  if (options.value.typeFolder)
    datalists.value = datalists.value.filter(
      (x) => x.is_folder == (options.value.typeFolder == "folder")
    );
  if (options.value.capacity_filter) {
    switch (options.value.typeUnit) {
      case "Bytes":
        if (options.value.condition == "<")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) <
              options.value.capacity_filter
          );
        else if (options.value.condition == "<=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) <=
              options.value.capacity_filter
          );
        else if (options.value.condition == "=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) ==
              options.value.capacity_filter
          );
        else if (options.value.condition == ">")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) >
              options.value.capacity_filter
          );
        else if (options.value.condition == ">=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) >=
              options.value.capacity_filter
          );
        break;
      case "KB":
        if (options.value.condition == "<")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) <
              options.value.capacity_filter * 1024
          );
        else if (options.value.condition == "<=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) <=
              options.value.capacity_filter * 1024
          );
        else if (options.value.condition == "=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) ==
              options.value.capacity_filter * 1024
          );
        else if (options.value.condition == ">")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) >
              options.value.capacity_filter * 1024
          );
        else if (options.value.condition == ">=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) >=
              options.value.capacity_filter * 1024
          );
        break;
      case "MB":
        if (options.value.condition == "<")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) <
              options.value.capacity_filter * 1024 * 1024
          );
        else if (options.value.condition == "<=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) <=
              options.value.capacity_filter * 1024 * 1024
          );
        else if (options.value.condition == "=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) ==
              options.value.capacity_filter * 1024 * 1024
          );
        else if (options.value.condition == ">")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) >
              options.value.capacity_filter * 1024 * 1024
          );
        else if (options.value.condition == ">=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) >=
              options.value.capacity_filter * 1024 * 1024
          );
        break;
      case "GB":
        if (options.value.condition == "<")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) <
              options.value.capacity_filter * 1024 * 1024 * 1024
          );
        else if (options.value.condition == "<=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) <=
              options.value.capacity_filter * 1024 * 1024 * 1024
          );
        else if (options.value.condition == "=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) ==
              options.value.capacity_filter * 1024 * 1024 * 1024
          );
        else if (options.value.condition == ">")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) >
              options.value.capacity_filter * 1024 * 1024 * 1024
          );
        else if (options.value.condition == ">=")
          datalists.value = datalists.value.filter(
            (x) =>
              formatCapacity(x.capacity, x.is_folder) >=
              options.value.capacity_filter * 1024 * 1024 * 1024
          );
        break;
    }
  }
}
function formatCapacity(value, type) {
  return type ? value * 1024 * 1024 : value; // format ve byte
}
const filterFile = () => {
  checkFilter.value = true;
    datalists.value = datalists_temp;
  FilterAndSearch();
  filterButs.value.toggle();
};
const refilterFile = () => {
  options.value.typeFolder = null;
  options.value.capacity_filter = null;
  options.value.search = null;
  options.value.condition = ">";
  options.value.typeUnit = "MB";
  checkFilter.value = false;
  is_search.value = false;
  datalists.value = datalists_temp;
  //filterButs.value.toggle();
};
const ChangeSortFile = (type_sort) => {
  itemSortButs.value.forEach((i) => {
    if (i.type_sort == type_sort) {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  menuSortButs.value.toggle();
  switch (type_sort) {
    case "created_date_desc": //Ngày tạo mới đến cũ
      //datalists.value.sort((a,b)=> new Date(b.created_date)- new(a.created_date))

      datalists.value
        .sort(function (a, b) {
          return new Date(b.created_date) - new Date(a.created_date);
        })
        .sort((a, b) => b.is_folder - a.is_folder);
      break;
    case "created_date_asc": //Ngày tạo cũ đến mới
      datalists.value
        .sort(function (a, b) {
          return new Date(a.created_date) - new Date(b.created_date);
        })
        .sort((a, b) => b.is_folder - a.is_folder);
      break;
    case "capacity_asc": //Kích thước tăng dần
      datalists.value
        .sort((a, b) => a.capacity - b.capacity)
        .sort((a, b) => b.is_folder - a.is_folder);
      break;
    case "capacity_desc": //Kích thước giảm dần
      datalists.value
        .sort((a, b) => b.capacity - a.capacity)
        .sort((a, b) => b.is_folder - a.is_folder);
      break;
    case "file_name_asc": //Tên từ A-Z
      datalists.value
        .sort(function (a, b) {
          return a.file_name === b.file_name
            ? 0
            : a.file_name > b.file_name
            ? 1
            : -1;
        })
        .sort((a, b) => b.is_folder - a.is_folder);
      break;
    case "file_name_desc": //Tên từ Z-A
      datalists.value
        .sort(function (a, b) {
          return a.file_name === b.file_name
            ? 0
            : a.file_name > b.file_name
            ? -1
            : 1;
        })
        .sort((a, b) => b.is_folder - a.is_folder);
  }
  // loadData(true, opition.value.type_view);
};
function isEmpty(val) {
  return val === undefined || val == null || val.length <= 0 ? true : false;
}
function removeVietnameseTones(str) {
  str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
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
  // Some system encode vietnamese combining accent as individual utf-8 characters
  // Một vài bộ encode coi các dấu mũ, dấu chữ như một kí tự riêng biệt nên thêm hai dòng này
  str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, ""); // ̀ ́ ̃ ̉ ̣  huyền, sắc, ngã, hỏi, nặng
  str = str.replace(/\u02C6|\u0306|\u031B/g, ""); // ˆ ̆ ̛  Â, Ê, Ă, Ơ, Ư
  // Remove extra spaces
  // Bỏ các khoảng trắng liền nhau
  str = str.replace(/ + /g, " ");
  str = str.trim();
  // Remove punctuations
  // Bỏ dấu câu, kí tự đặc biệt
  str = str.replace(
    /!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g,
    " "
  );
  return str;
}

//var pressedKeys = {};
const pressedKeys = ref({
  Control: false,
});
window.onkeyup = function (e) {
  pressedKeys.value[e.keyCode] = false;
};
window.onkeydown = function (e) {
  pressedKeys.value[e.keyCode] = true;
};
onMounted(() => {
  resize_ob.observe(document.querySelector("#demo-textarea"));
  if (cookies.get("max_length_file") != null)
    max_length_file.value = parseInt(cookies.get("max_length_file"));
  initModulesMenu();
  // loadTudien(true);
});
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "choiceusers":
      displayDialogUser.value = obj.data["displayDialog"];
      if (obj.data["submit"]) {
        saveShareFile(obj.data.selected, obj.data.is_module);
      }
      break;
    case "choicefolder":
      displayShareFolder.value = obj.data["displayDialog"];
      if (obj.data["submit"]) {
        file_edit.value.type_share = obj.data.type_share;
        saveShareFolder(obj.data.selected);
      }
      break;
  }
});
</script>
<script>
import { ref } from "vue";
const item_clicked = ref();
export default {
  setup() {},
  methods: {
    handleClick(event, item) {
      item_clicked.value = item;
      this.$refs.vueSimpleContextMenu1.showMenu(event, item);
    },
  },
};
</script>
<template>
  <div>
    <div class="w-full">
      <Splitter class="w-full">
        <SplitterPanel :size="20" style="min-width: 150px">
          <div style="height: calc(100vh - 60px)">
            <TreeTable
              :value="folder_tree"
              @nodeSelect="onNodeSelect"
              @nodeUnselect="onNodeUnselect"
              selectionMode="single"
              v-model:selectionKeys="selectedKey"
              class="h-full w-full overflow-x-hidden p-0"
              scrollHeight="flex"
              responsiveLayout="scroll"
              :scrollable="true"
              :expandedKeys="expandedKeys"
            >
              <Column field="name" :expander="true" class="cursor-pointer flex">
                <template #header>
                  <Toolbar class="w-full p-0 border-none sticky top-0">
                    <template #start>
                      <div class="font-bold text-xl">Danh mục kho dữ liệu</div>
                    </template>
                  </Toolbar>
                </template>

                <template #body="data">
                  <div class="relative flex w-full p-0" v-if="data.node.data">
                    <div class="grid w-full p-0">
                      <div
                        class="field col-12 md:col-12 w-full flex m-0 p-0 pt-2"
                      >
                        <div class="col-2 p-0">
                          <img
                            :src="basedomainURL + data.node.data.is_filepath"
                            width="28"
                            height="36"
                            style="object-fit: contain"
                          />
                        </div>
                        <div
                          class="col-10 p-0 flex"
                          style="align-items: center"
                        >
                          <div class="px-2" style="line-height: 20px">
                            {{ data.node.name }} ({{ data.node.count_child }})
                            <!-- <span v-if="data.node.children.length > 0"
                              >({{ data.node.children.length }})</span
                            > -->
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </template>
              </Column>
            </TreeTable>
          </div>
        </SplitterPanel>
        <SplitterPanel :size="80">
          <div id="demo-textarea">
            <Toolbar class="w-full">
              <template #start>
                <div class="flex w-full">
                  <div v-if="folder">
                    <Button
                      v-if="
                        folder.folder_id !== 'me' &&
                        folder.folder_id !== 'share' &&
                        folder.module_key !== 'M0'
                      "
                      label="Quay lại"
                      icon="pi pi-arrow-left"
                      class="p-button-outlined p-button-secondary"
                      @click="goPrevious(folder)"
                    />
                  </div>
                  <div
                    class="
                      p-input-icon-left
                      flex flex-column flex-grow-1
                      format-center
                    "
                  >
                    <div class="p-input-icon-left">
                      <i
                        :class="
                          checkFilter
                            ? 'pi pi-filter-fill color-filter'
                            : 'pi pi-filter'
                        "
                        class="cursor-pointer"
                        @click="toggleFilter"
                        aria-haspopup="true"
                        aria-controls="overlay_panelS"
                      />
                      <InputText
                        v-model="options.search"
                        type="text"
                        spellcheck="false"
                        placeholder="Tìm kiếm"
                        v-on:keyup.enter="onSearch"
                        style="width: 23vw"
                      />
                      <OverlayPanel
                        ref="filterButs"
                        appendTo="body"
                        :showCloseIcon="false"
                        id="overlay_panelS"
                        style="width: 400px"
                        :breakpoints="{ '960px': '20vw' }"
                      >
                        <div class="grid formgrid m-2">
                          <div
                            class="
                              field
                              col-12
                              md:col-12
                              flex
                              align-items-center
                            "
                          >
                            <div class="col-4 p-0">Loại:</div>
                            <Dropdown
                              :showClear="true"
                              v-model="options.typeFolder"
                              :options="loais"
                              optionLabel="name"
                              optionValue="value"
                              placeholder="Chọn loại"
                              class="p-dropdown-sm col-8 p-0"
                            />
                          </div>
                          <div
                            class="
                              field
                              col-12
                              md:col-12
                              flex
                              align-items-center
                            "
                          >
                            <div class="col-4 p-0">Kích thước:</div>
                            <InputNumber
                              class="col-8 ip32 p-0"
                              v-model="options.capacity_filter"
                              :useGrouping="false"
                              placeholder="Nhập kích thước"
                            />
                          </div>
                          <div
                            class="
                              field
                              col-12
                              md:col-12
                              flex
                              align-items-center
                            "
                          >
                            <div class="col-4 p-0"></div>
                            <Dropdown
                              :showClear="false"
                              v-model="options.condition"
                              :options="conditions"
                              optionLabel="name"
                              optionValue="value"
                              placeholder="Điều kiện"
                              class="p-dropdown-sm col-8 p-0"
                            />
                          </div>
                          <div
                            class="
                              field
                              col-12
                              md:col-12
                              flex
                              align-items-center
                            "
                          >
                            <div class="col-4 p-0"></div>
                            <Dropdown
                              :showClear="true"
                              v-model="options.typeUnit"
                              :options="units"
                              optionLabel="name"
                              optionValue="value"
                              placeholder="Đơn vị"
                              class="p-dropdown-sm col-8 p-0"
                            />
                          </div>
                          <small
                            class="col-12 p-error p-0"
                            v-if="
                              checkFilter &&
                              options.capacity_filter &&
                              !options.typeUnit
                            "
                          >
                            <label class="col-4 text-left"></label>
                            <span class="col-8">Chọn đơn vị tính </span>
                          </small>
                          <div class="col-12 field p-0">
                            <Toolbar class="toolbar-filter">
                              <template #start>
                                <Button
                                  @click="refilterFile"
                                  class="p-button-outlined"
                                  label="Xóa"
                                ></Button>
                              </template>
                              <template #end>
                                <Button
                                  @click="filterFile"
                                  label="Lọc"
                                ></Button>
                              </template>
                            </Toolbar>
                          </div>
                        </div>
                      </OverlayPanel>
                    </div>
                  </div>
                </div>
              </template>
              <template #end>
                <Button
                  label="Thêm tài liệu"
                  icon="pi pi-plus"
                  class="mr-2"
                  @click="showModalAddFile"
                />
                <Button
                  label="Xoá"
                  icon="pi pi-trash"
                  class="mr-2 p-button-danger btn-delete"
                  v-if="active_delete || isShowBtnDel"
                  @click="delListFiles(listItemClicked)"
                />
                <DataViewLayoutOptions v-model="layout" />
                <div
                  @click="toggleSort"
                  aria-haspopup="true"
                  aria-controls="overlay_Export"
                  class="btn_sort ml-3 px-1 cursor-pointer"
                >
                  <a
                    ><i class="pi pi-sort"></i> Sắp xếp
                    <i class="pi pi-angle-down"></i
                  ></a>
                </div>
                <Button
                  class="mr-2 ml-2 p-button-outlined p-button-secondary"
                  icon="pi pi-refresh"
                  @click="onRefresh"
                />
                <Menu
                  class="menu_sort"
                  :model="itemSortButs"
                  ref="menuSortButs"
                  :popup="true"
                >
                  <template #item="{ item }">
                    <a
                      @click="ChangeSortFile(item.type_sort)"
                      :class="{ active: item.active }"
                      >{{ item.label }}</a
                    >
                  </template>
                </Menu>
              </template>
            </Toolbar>
          </div>
          <div
            ref="target"
            class="col-12 p-0 overflow-y-auto"
            :class="layout == 'grid' && datalists.length > 0 ? item_grid : ''"
            style="height: calc(100vh - 115px)"
          >
            <DataView
              class="w-full h-full e-sm flex flex-column p-dataview-unset"
              :value="datalists"
              :layout="layout"
              :paginator="datalists.length > (layout == 'grid' ? '36' : '10')"
              :rows="layout == 'grid' ? '36' : '10'"
              responsiveLayout="scroll"
              :scrollable="false"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
              v-model:first="first"
            >
              <template #header>
                <!-- :home="homeBread" -->
                <Breadcrumb
                  v-if="itemBreads.length > 0"
                  :home="itemBreads[0]"
                  :model="
                    itemBreads.length > 1
                      ? itemBreads.filter(
                          (x) =>
                            x.folder_id !== 'me' &&
                            x.folder_id !== 'share' &&
                            x.folder_id !== '1'
                        )
                      : []
                  "
                  style="padding: 10px; background-color: #fff; border: none"
                >
                  <template #item="{ item }">
                    <Button
                      style="color: inherit; font-size: 12.8px !important"
                      class="p-button-text p-0"
                      @click="
                        loadThumuc(
                          item.folder_id,
                          item.is_shared,
                          item.module_key
                        )
                      "
                    >
                      <img
                        style="vertical-align: bottom; margin-right: 5px"
                        src="../../assets/image/file/folder480.svg"
                        height="24"
                      />
                      {{ item.label }}
                    </Button>
                  </template>
                </Breadcrumb>
                              <div class="col-12 flex" v-if="is_search">
                <div class="col-6 text-lg">
                  <span class="">Kết quả cho </span>
                  <span class="font-bold font-italic" v-if="options.search">{{
                    options.search
                  }}</span>
                  <span
                    v-if="
                      (options.search && options.typeFolder) ||
                      (options.search && options.capacity_filter)
                    "
                    >,
                  </span>
                  <span
                    class="font-bold font-italic"
                    v-if="options.typeFolder"
                    >{{
                      loais.filter((x) => x.value == options.typeFolder)[0].name
                    }}</span
                  >
                  <span v-if="options.typeFolder && options.capacity_filter"
                    >,
                  </span>
                  <span
                    class="font-bold font-italic"
                    v-if="options.capacity_filter"
                    >{{ options.condition }}{{ options.capacity_filter
                    }}{{ options.typeUnit }}</span
                  >
                </div>
                <div class="col-6">
                  <Button
                    icon="pi pi-times"
                    class="p-button-rounded p-button-danger p-button-outlined"
                    style="float: right"
                    v-tooltip.top="'Xóa'"
                    @click="refilterFile()"
                  />
                </div>
              </div>
              </template>
              <template #grid="slotProps">
                <div
                  style="width: 100%"
                  v-if="!slotProps.data.is_folder"
                  class="md:col-2 col-2 card-content"
                  :class="
                    listItemClicked.filter(
                      (x) => x.file_id == slotProps.data.file_id
                    ).length > 0
                      ? 'active-click'
                      : ''
                  "
                  @click="clickItemFile(slotProps.data)"
                  v-on:dblclick="
                    slotProps.data.is_image
                      ? dbImageClick(slotProps.data)
                      : dbFileClick(slotProps.data);
                    active_delete = false;
                  "
                  :title="slotProps.data.labelContext"
                  @contextmenu.prevent.stop="
                    handleClick($event, slotProps.data)
                  "
                >
                  <Card class="no-paddcontent p-0">
                    <template #title>
                      <div style="position: relative" class="grid-item">
                        <Image
                          v-if="slotProps.data.is_image"
                          height="110"
                          class="w-full cursor-pointer"
                          v-bind:src="
                            slotProps.data.is_filepath
                              ? basedomainURL + slotProps.data.is_filepath
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                        />
                        <img
                          v-else
                          class="w-full cursor-pointer"
                          style="height: 110px; object-fit: contain"
                          v-bind:src="
                            basedomainURL +
                            '/Portals/file/' +
                            slotProps.data.file_type.replace('.','') +
                            '.png'
                          "
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                        />
                      </div>
                    </template>
                    <template #content>
                      <div
                        class="
                          format-center
                          font-semibold
                          mx-2
                          text-3line text-title
                          my-2
                        "
                      >
                        {{ slotProps.data.file_name }}
                      </div>
                    </template>
                  </Card>
                </div>
                <div
                  style="width: 100%"
                  v-if="slotProps.data.is_folder"
                  :title="slotProps.data.labelContext"
                  class="md:col-2 col-2 card-content"
                  :class="
                    listItemClicked.filter(
                      (x) => x.file_id == slotProps.data.file_id
                    ).length > 0
                      ? 'active-click'
                      : ''
                  "
                  @click="clickItemFile(slotProps.data)"
                  v-on:dblclick="
                    onNodeSelect(slotProps);
                    active_delete = false;
                  "
                  @contextmenu.prevent.stop="
                    handleClick($event, slotProps.data)
                  "
                >
                  <Card class="no-paddcontent p-0">
                    <template #title>
                      <div style="" class="flex format-center">
                        <img
                          class="col-12"
                          style="height: 110px; object-fit: contain"
                          v-bind:src="
                            basedomainURL + '/Portals/Image/folder.png'
                          "
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                        />
                      </div>
                    </template>
                    <template #content>
                      <div
                        class="
                          format-center
                          font-semibold
                          mx-2
                          text-3line text-title
                          my-2
                        "
                      >
                        {{ slotProps.data.file_name }}
                      </div>
                    </template>
                  </Card>
                </div>
              </template>
              <template #list="slotProps">
                <div
                  class="p-2 w-full"
                  :class="slotProps.data.chon ? 'item-clicked' : ''"
                >
                  <div class="flex align-items-center justify-content-center">
                    <div class="mx-2">
                      <Checkbox
                        v-if="
                          !slotProps.data.is_module && !slotProps.data.is_share
                        "
                        id="IsIdentity"
                        v-model="slotProps.data.chon"
                        :binary="true"
                        @change="clickDelFile"
                      />
                    </div>
                    <Image
                      v-on:dblclick="
                        slotProps.data.is_folder
                          ? onNodeSelect(slotProps)
                          : slotProps.data.is_image
                          ? dbImageClick(slotProps.data)
                          : dbFileClick(slotProps.data)
                      "
                      v-if="slotProps.data.is_image || slotProps.data.is_folder"
                      height="40"
                      class="cursor-pointer"
                      v-bind:src="
                        slotProps.data.is_filepath
                          ? basedomainURL + slotProps.data.is_filepath
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                    />
                    <img
                      v-else
                      v-on:dblclick="
                        slotProps.data.is_folder
                          ? onNodeSelect(slotProps)
                          : slotProps.data.is_image
                          ? dbImageClick(slotProps.data)
                          : dbFileClick(slotProps.data)
                      "
                      class="cursor-pointer"
                      style="height: 40px; object-fit: contain"
                      v-bind:src="
                        basedomainURL +
                        '/Portals/file/' +
                        slotProps.data.file_type.replace('.','') +
                        '.png'
                      "
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                    />
                    <div
                      class="flex flex-column flex-grow-1 ml-4"
                      v-on:dblclick="
                        slotProps.data.is_folder
                          ? onNodeSelect(slotProps)
                          : slotProps.data.is_image
                          ? dbImageClick(slotProps.data)
                          : dbFileClick(slotProps.data)
                      "
                    >
                      <div style="color: inherit; padding: 0 !important">
                        <h3 class="mb-1 mt-0">
                          {{ slotProps.data.file_name }}
                        </h3>
                        <span style="font-size: 10pt; color: #999">{{
                          slotProps.data.created_name
                        }}</span>
                      </div>
                    </div>
                    <Chip
                      v-if="
                        slotProps.data.capacity && !slotProps.data.is_folder
                      "
                      class="ml-2 mr-2"
                      :label="slotProps.data.capacityMB"
                    />
                    <Chip
                      v-if="slotProps.data.capacity && slotProps.data.is_folder"
                      class="ml-2 mr-2"
                      :class="
                        slotProps.data.ratioMB < 70
                          ? ''
                          : slotProps.data.ratioMB < 90
                          ? 'bg-info-folder'
                          : 'bg-warning-folder'
                      "
                      :label="
                        (slotProps.data.capacityMB || 0) +
                        '/' +
                        slotProps.data.capacity +
                        ' MB'
                      "
                    />
                    <div style="background-color: #eee; font-size: 10pt">
                      {{
                        moment(new Date(slotProps.data.created_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </div>
                    <Button
                      v-if="slotProps.data.file_type"
                      v-bind:label="slotProps.data.file_type.toUpperCase()"
                      v-bind:class="'ml-2 mr-2 p-button p-button-warning p-button-rounded'"
                    />
                    <Button
                      icon="pi pi-ellipsis-h"
                      class="p-button-outlined p-button-secondary ml-2"
                      @click="toggleMores($event, slotProps.data)"
                      aria-haspopup="true"
                      aria-controls="overlay_More"
                      style="min-width: 30px"
                    />
                  </div>
                </div>
              </template>
              <template #empty>
                <div
                  class="
                    align-items-center
                    justify-content-center
                    p-4
                    text-center
                  "
                >
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
            </DataView>          
          </div>
        </SplitterPanel>
      </Splitter>
    </div>
    <div>
      <Dialog
        v-model:visible="displayAddFile"
        header="Cập nhật tài liệu"
        :modal="true"
        :closable="true"
        :style="{ width: '46vw', zIndex: 1000 }"
        :maximizable="true"
        :autoZIndex="true"
      >
        <div class="grid formgrid m-2">
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"
              >Tên tài liệu <span class="redsao">(*)</span></label
            >
            <InputText
              spellcheck="false"
              class="col-10 ip36"
              v-model="filemain.file_name"
            />
          </div>

          <small
            v-if="
              (v$.file_name.required.$invalid && submitted) ||
              v$.file_name.required.$pending.$response
            "
            class="col-12 p-error"
          >
            <div class="field col-12 md:col-12">
              <label class="col-2"></label>
              <span class="col-10 p-0">{{
                v$.file_name.required.$message
                  .replace("Value", "Tên tài liệu")
                  .replace("is required", "không được để trống")
              }}</span>
            </div>
          </small>
          <small
            v-if="v$.file_name.maxLength.$invalid && submitted"
            class="col-12 p-error"
          >
            <div class="field col-12 md:col-12">
              <label class="col-2"></label>
              <span class="col-10 p-0"
                >{{
                  v$.file_name.maxLength.$message.replace(
                    "The maximum length allowed is",
                    "Tên tài liệu không được vượt quá"
                  )
                }}
                ký tự</span
              >
            </div>
          </small>
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left">Thư mục</label>
            <TreeSelect
              class="col-10"
              v-model="selectCapcha"
              :options="treefolders"
              :showClear="true"
              placeholder=""
              optionLabel="data.folder_name"
              optionValue="data.folder_id"
            >
            </TreeSelect>
          </div>
          <div class="field col-12 md:col-12">
            <label class="text-left col-2">Từ khóa </label>
            <Chips
              class="col-10 p-0"
              v-model="filemain.keywords_mode"
              placeholder="Nhấn Enter để thêm"
            />
          </div>
          <div class="field col-12 md:col-12 flex">
            <label class="text-left col-2"
              >File upload <span class="redsao">(*)</span></label
            >
            <div class="col-10 p-0">
              <FileUpload
                chooseLabel="Chọn File"
                :showUploadButton="false"
                :showCancelButton="false"
                :multiple="false"
                :fileLimit="1"
                accept=""
                @select="onUploadFile"
                @remove="removeFile"
                ref="uploadFile"
              >
                <template #empty>
                  <div
                    class="
                      flex
                      align-items-center
                      justify-content-center
                      flex-column
                    "
                    @click="onUploadFile"
                  >
                    <i
                      class="
                        pi pi-cloud-upload
                        border-2 border-circle
                        p-5
                        text-6xl text-400
                        border-400
                      "
                    />
                    <p class="mt-4 mb-0">Hoặc kéo thả vào đây để upload file</p>
                  </div>
                </template>
              </FileUpload>
            </div>
          </div>
          <div
            class="col-12 p-0 flex field"
            v-if="filemain.is_filepath && !isHiddenFile"
          >
            <label class="col-2"></label>
            <div class="item-video col-10">
              <Toolbar class="w-full py-3">
                <template #start>
                  <div class="flex align-items-center">
                    <img
                      :src="
                        basedomainURL +
                        '/Portals/file/' +
                        filemain.file_type +
                        '.png'
                      "
                      style="object-fit: contain"
                      width="50"
                      height="50"
                    />
                    <span style="line-height: 20px; word-break: break-word">
                      {{ filemain.name_file || filemain.file_name }}</span
                    >
                  </div>
                </template>
                <template #end>
                  <Button
                    icon="pi pi-ellipsis-h"
                    class="p-button-rounded p-button-text ml-2"
                    @click="toggleFile($event)"
                    aria-haspopup="true"
                    aria-controls="overlay_File"
                  />
                  <!-- <Button
                    icon="pi pi-times"
                    class="p-button-rounded p-button-danger"
                    @click="deleteFileCode(item)"
                  /> -->
                </template>
              </Toolbar>
            </div>
          </div>
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left">STT</label>
            <InputNumber class="col-2 ip36 p-0" v-model="filemain.is_order" />
            <label class="col-2"></label>
            <label class="col-2 text-right">Trạng thái</label>
            <InputSwitch v-model="filemain.status" />
          </div>
        </div>
        <template #footer>
          <Button
            label="Thoát"
            icon="pi pi-times"
            class="p-button-text"
            @click="displayAddFile = false"
          />
          <Button
            label="Lưu"
            icon="pi pi-check"
            @click="SaveFile(!v$.$invalid)"
          />
        </template>
      </Dialog>
      <Dialog
        v-model:visible="displayAddFolder"
        header="Cập nhật thư mục"
        :modal="true"
        :closable="true"
        :style="{ width: '46vw', zIndex: 1000 }"
        :maximizable="true"
        :autoZIndex="true"
      >
        <div class="grid formgrid m-2">
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"
              >Tên thư mục <span class="redsao">(*)</span></label
            >
            <InputText
              spellcheck="false"
              class="col-10 ip36"
              v-model="foldermain.folder_name"
            />
          </div>
          <small
            v-if="
              (t$.folder_name.required.$invalid && submitted) ||
              t$.folder_name.required.$pending.$response
            "
            class="col-12 p-error"
          >
            <div class="field col-12 md:col-12">
              <label class="col-2"></label>
              <span class="col-10 p-0">{{
                t$.folder_name.required.$message
                  .replace("Value", "Tên thư mục")
                  .replace("is required", "không được để trống")
              }}</span>
            </div>
          </small>
          <small
            v-if="t$.folder_name.maxLength.$invalid && submitted"
            class="col-12 p-error"
          >
            <div class="field col-12 md:col-12">
              <label class="col-2"></label>
              <span class="col-10 p-0"
                >{{
                  t$.folder_name.maxLength.$message.replace(
                    "The maximum length allowed is",
                    "Tên thư mục không được vượt quá"
                  )
                }}
                ký tự</span
              >
            </div>
          </small>
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left">Cấp cha</label>
            <TreeSelect
              class="col-10"
              v-model="selectCapcha"
              :options="treefolders"
              :showClear="true"
              placeholder=""
              optionLabel="data.folder_name"
              optionValue="data.folder_id"
            >
            </TreeSelect>
          </div>
          <div class="field col-12 md:col-12">
            <label class="text-left col-2">Từ khóa </label>
            <Chips
              class="col-10 p-0"
              v-model="foldermain.keywords"
              placeholder="Nhấn Enter để thêm"
            />
          </div>
          <div class="field col-12 md:col-12 flex">
            <label class="text-left col-2">Mô tả </label>
            <Textarea
              style="border-radius: 5px; padding: 0.5rem"
              class="col-10"
              spellcheck="false"
              :autoResize="true"
              rows="3"
              v-model="foldermain.des"
            />
          </div>
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left">Dung lượng lưu trữ (MB)</label>
            <InputNumber class="col-1 ip36 p-0" v-model="foldermain.capacity" />
            <label class="col-2 text-right">STT</label>
            <InputNumber class="col-1 ip36 p-0" v-model="foldermain.is_order" />

            <label class="col-2 text-right">Trạng thái</label>
            <InputSwitch v-model="foldermain.status" />
          </div>
        </div>
        <template #footer>
          <Button
            label="Thoát"
            icon="pi pi-times"
            class="p-button-text"
            @click="displayAddFolder = false"
          />
          <Button
            label="Lưu"
            icon="pi pi-check"
            @click="saveFolder(!t$.$invalid)"
          />
        </template>
      </Dialog>
      <Dialog
        v-model:visible="ModalDetail"
        header="Chi tiết"
        :modal="true"
        :closable="true"
        :style="{ width: '70vw' }"
        :maximizable="true"
        :autoZIndex="true"
      >
        <div class="grid formgrid m-2 h-full">
          <div v-if="DataDetail" class="w-full format-center">
                        <img
              v-if="
                'gif,jpeg,png,jpg,.gif,.jpeg,.png,.jpg'.includes(DataDetail.file_type.toLowerCase())
              "
              style="width: 100%; min-height: 66vh; height: 100%"
              class="w-full cursor-pointer"
              :src="
                DataDetail.is_filepath
                  ? basedomainURL + DataDetail.is_filepath
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              "
            />
            <video
              v-if="
                'mp4,flv,mov,wmv,.mp4,.flv,.mov,.wmv'.includes(DataDetail.file_type.toLowerCase())
              "
              style="width: 100%; min-height: 66vh; height: 100%"
              controls
              :src="basedomainURL + DataDetail.is_filepath"
            ></video>
            <audio
              style="width: 100%; margin: 0px auto"
              controls
              v-if="
                'mp3,wma,aac,flac,alac,wav,.mp3,.wma,.aac,.flac,.alac,.wav'.includes(
                  DataDetail.file_type.toLowerCase()
                )
              "
            >
              <source :src="basedomainURL + DataDetail.is_filepath" />
            </audio>
            <iframe
              v-if="
                'pptx,ppt,doc,docx,xls,xlsx, pdf, txt, .pptx,.ppt,.doc,.docx,.xls,.xlsx,.pdf,.txt'.includes(
                  DataDetail.file_type.toLowerCase()
                )
              "
              allowfullscreen
              :src="
                basedomainURL +
                '/Viewer/?title=' +
                DataDetail.file_name +
                '&url=' +
                DataDetail.is_filepath
              "
              style="width: 100%; min-height: 66vh; height: 100%"
              title="Iframe Example"
            >
            </iframe>
          </div>
          <div v-else class="w-full">
            <div class="col-12 format-center">
              <img
                :src="basedomainURL + 'Portals/file/94992-error-404.gif'"
                style="width: 400px"
              />
            </div>
            <div class="col-12 format-center no-permission">
              Bạn không có quyền xem tài liệu này!
            </div>
          </div>
        </div>
      </Dialog>
      <Dialog
        v-model:visible="displayShowFileNoti"
        header="Chi tiết"
        :modal="true"
        :closable="true"
        :style="{ width: '70vw' }"
        :maximizable="true"
        :autoZIndex="true"
      >
        <div class="grid formgrid m-2 h-full">
          <div v-if="dataInfoNoti" class="w-full">
            <video
              v-if="
                'mp4,flv,mov,wmv'.includes(dataInfoNoti.file_type.toLowerCase())
              "
              style="width: 100%; height: 60vh; height: 100%"
              controls
              :src="basedomainURL + dataInfoNoti.is_filepath"
            ></video>
            <audio
              style="width: 100%; margin: 0px auto"
              controls
              v-if="
                'mp3,wma,aac,flac,alac,wav'.includes(
                  dataInfoNoti.file_type.toLowerCase()
                )
              "
            >
              <source :src="basedomainURL + dataInfoNoti.is_filepath" />
            </audio>
            <iframe
              v-if="
                'pptx,ppt,doc,docx,xls,xlsx, pdf, txt'.includes(
                  dataInfoNoti.file_type.toLowerCase()
                )
              "
              allowfullscreen
              :src="
                basedomainURL +
                '/Viewer/?title=' +
                dataInfoNoti.file_name +
                '&url=' +
                dataInfoNoti.is_filepath
              "
              style="width: 100%; min-height: 66vh; height: 100%"
              title="Iframe Example"
            >
            </iframe>
            <Image
              v-if="dataInfoNoti.is_image"
              height="110"
              class="w-full cursor-pointer"
              v-bind:src="
                dataInfoNoti.is_filepath
                  ? basedomainURL + dataInfoNoti.is_filepath
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              "
            />
          </div>
        </div>
      </Dialog>
      <div v-if="displayShareFolder">
        <foldershare
          :displayDialog="displayShareFolder"
          :folder="file_edit.file_id"
        />
      </div>
      <div v-if="displayDialogUser">
        <treeuser
          :headerDialog="headerDialogUser"
          :displayDialog="displayDialogUser"
          :isModule="file_edit.is_module"
          :selected="file_edit.is_module ? null : file_edit.file_id"
          :idKey="file_edit.is_module ? file_edit.file_id.toString() : null"
          :moduleKey="file_edit.is_module ? file_edit.module_key : null"
        />
      </div>
      <Sidebar
        v-model:visible="showInfoFileModule"
        :baseZIndex="100"
        position="right"
        class="p-sidebar-lg overflow-hidden sidebar-noti"
        style="width: 38vw"
        :showCloseIcon="false"
        @hide="showInfoFileModule = false"
      >
        <div class="grid w-full p-0">
          <div class="col-12">
            <h2 style="margin: 4px auto; font-size: 1.5rem">
              Thông tin {{ file_info.is_folder ? "thư mục" : "tài liệu" }}
            </h2>
          </div>
          <div
            style="max-height: calc(100vh - 66px); overflow: auto"
            class="col-12"
          >
            <div class="field col-12 md:col-12 flex">
              <div class="col-4 format-center">
                <div>
                  <img
                    v-if="file_info.is_image || file_info.is_folder"
                    style="height: 120px"
                    class="w-full cursor-pointer"
                    v-bind:src="
                      file_info.is_filepath
                        ? basedomainURL + file_info.is_filepath
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                  />
                  <img
                    v-else
                    style="height: 120px"
                    class="w-full cursor-pointer"
                    v-bind:src="
                      basedomainURL +
                      '/Portals/file/' +
                      file_info.file_type +
                      '.png'
                    "
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                  />
                </div>
              </div>
              <div class="col-8 pl-3" style="font-size: 14px">
                <div class="col-12 p-1">
                  <i class="pi pi-folder mr-1" style="font-size: 14px"></i>
                  Tên {{ file_info.is_folder ? "thư mục" : "file" }} :
                  <span class="font-medium">{{ file_info.file_name }}</span>
                </div>
                <div class="col-12 p-1" v-if="file_info.is_folder">
                  <i class="pi pi-file mr-1" style="font-size: 14px"></i>
                  Số file :
                  <span class="font-medium"> {{ file_info.count_files }}</span>
                </div>
                <div class="col-12 p-1" v-if="file_info.count_size">
                  <i class="pi pi-chart-bar mr-1" style="font-size: 14px"></i>
                  Kích thước :
                  <span class="font-medium">
                    {{ file_info.count_size_MB }}</span
                  >
                </div>
                <div class="col-12 p-1" v-if="!file_info.is_folder">
                  <i class="pi pi-file mr-1" style="font-size: 14px"></i>
                  Loại :
                  <span class="font-medium" v-if="file_info.file_type">
                    {{ file_info.file_type.toUpperCase() }}</span
                  >
                </div>
                <div
                  class="col-12 p-1"
                  v-if="file_info.capacity && !file_info.is_folder"
                >
                  <i class="pi pi-chart-bar mr-1" style="font-size: 14px"></i>
                  Kích thước :
                  <span class="font-medium"> {{ file_info.capacityMB }}</span>
                </div>
                <div
                  class="col-12 p-1"
                  v-if="file_info.capacity && file_info.is_folder"
                >
                  <i class="pi pi-chart-bar mr-1" style="font-size: 14px"></i>
                  Kích thước :
                  <span class="font-medium"> {{ file_info.capacity }}MB</span>
                </div>
                <div class="col-12 p-1" v-if="file_info.created_date">
                  <i class="pi pi-calendar mr-1" style="font-size: 14px"></i>
                  Ngày tạo :
                  <span class="font-medium">
                    {{
                      moment(new Date(file_info.created_date)).format(
                        "DD/MM/YYYY HH:mm"
                      )
                    }}</span
                  >
                </div>
                <div
                  class="col-12 p-1"
                  v-if="file_info.user_share || file_info.created_name"
                >
                  <i class="pi pi-user mr-1" style="font-size: 14px"></i>
                  Người tạo :
                  <span class="font-medium">{{
                    file_info.user_share || file_info.created_name
                  }}</span>
                </div>
                <div
                  class="col-12 p-1"
                  v-if="file_info.capacity && file_info.is_folder"
                >
                  <div class="progress" style="height: 25px; width: 75%">
                    <div
                      class="progress-bar text-base"
                      :class="file_info.cssprogressbar"
                      role="progressbar"
                      :style="{ width: file_info.tyle_text }"
                      :aria-valuenow="file_info.tyle"
                      aria-valuemin="0"
                      aria-valuemax="100"
                    >
                      {{ file_info.tyle_text }}
                    </div>
                  </div>
                  <div
                    style="width: 75%; text-align: center"
                    class="text-sm font-semibold"
                  >
                    Sử dụng {{ file_info.capacity_used_MB }}/ tổng
                    {{ file_info.capacity }}MB
                  </div>
                </div>
              </div>
            </div>
            <div class="field col-12 md:col-12 text-center">
              <Button
                label="Chi tiết"
                icon="pi pi-angle-double-right"
                class=""
                @click="goToModule(file_info)"
              />
            </div>
          </div>
        </div>
      </Sidebar>
      <Sidebar
        v-model:visible="showSidebarInfo"
        :baseZIndex="100"
        position="right"
        class="p-sidebar-lg overflow-hidden sidebar-noti"
        style="width: 38vw"
        :showCloseIcon="false"
      >
        <div class="grid w-full p-0">
          <div class="col-12">
            <h2 style="margin: 4px auto; font-size: 1.5rem">
              Thông tin {{ file_info.is_folder ? "thư mục" : "tài liệu" }}
            </h2>
          </div>
          <div
            style="max-height: calc(100vh - 66px); overflow: auto"
            class="col-12"
          >
            <div class="field col-12 md:col-12 flex">
              <div class="col-4 format-center">
                <div>
                  <img
                    v-if="file_info.is_image || file_info.is_folder"
                    style="height: 120px"
                    class="w-full cursor-pointer"
                    v-bind:src="
                      file_info.is_filepath
                        ? basedomainURL + file_info.is_filepath
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                  />
                  <img
                    v-else
                    style="height: 120px"
                    class="w-full cursor-pointer"
                    v-bind:src="
                      basedomainURL +
                      '/Portals/file/' +
                      file_info.file_type +
                      '.png'
                    "
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                  />
                </div>
              </div>
              <div class="col-8 pl-3" style="font-size: 14px">
                <div class="col-12 p-1">
                  <i class="pi pi-folder mr-1" style="font-size: 14px"></i>
                  Tên {{ file_info.is_folder ? "thư mục" : "file" }} :
                  <span> {{ file_info.file_name }}</span>
                </div>
                <div class="col-12 p-1" v-if="!file_info.is_folder">
                  <i class="pi pi-file mr-1" style="font-size: 14px"></i>
                  Loại :
                  <span v-if="file_info.file_type"> {{ file_info.file_type.toUpperCase() }}</span>
                </div>
                <div
                  class="col-12 p-1"
                  v-if="file_info.capacityMB && !file_info.is_folder"
                >
                  <i class="pi pi-chart-bar mr-1" style="font-size: 14px"></i>
                  Kích thước :
                  <span> {{ file_info.capacityMB }}</span>
                </div>
                <div
                  class="col-12 p-1"
                  v-if="file_info.capacity && file_info.is_folder"
                >
                  <i class="pi pi-chart-bar mr-1" style="font-size: 14px"></i>
                  Kích thước :
                  <span> {{ file_info.capacity }}MB</span>
                </div>
                <div class="col-12 p-1" v-if="file_info.created_date">
                  <i class="pi pi-calendar mr-1" style="font-size: 14px"></i>
                  Ngày tạo :
                  <span>
                    {{
                      moment(new Date(file_info.created_date)).format(
                        "DD/MM/YYYY HH:mm"
                      )
                    }}</span
                  >
                </div>
                <div
                  class="col-12 p-1"
                  v-if="file_info.user_share || file_info.created_name"
                >
                  <i class="pi pi-user mr-1" style="font-size: 14px"></i>
                  Người tạo :
                  <span>{{
                    file_info.user_share || file_info.created_name
                  }}</span>
                </div>
                <div
                  class="col-12 p-1"
                  v-if="file_info.capacity && file_info.is_folder"
                >
                  <div class="progress" style="height: 25px; width: 75%">
                    <div
                      class="progress-bar text-base"
                      :class="file_info.cssprogressbar"
                      role="progressbar"
                      :style="{ width: file_info.tyle_text }"
                      :aria-valuenow="file_info.tyle"
                      aria-valuemin="0"
                      aria-valuemax="100"
                    >
                      {{ file_info.tyle_text }}
                    </div>
                  </div>
                  <div
                    style="width: 75%; text-align: center"
                    class="text-sm font-semibold"
                  >
                    Sử dụng {{ file_info.capacity_used_MB }}/ tổng
                    {{ file_info.capacity }}MB
                  </div>
                </div>
              </div>
            </div>
            <div class="field col-12 md:col-12 p-0">
              <div style="border-right: 1px solid #efefef">
                <ul
                  class="list-group ul-ktra"
                  style="flex-direction: row; justify-content: space-around"
                >
                  <li
                    class="list-group-item"
                    :class="typeTab == 1 ? 'active_li' : ''"
                    @click="typeTab = 1"
                  >
                    <i class="pi pi-history mr-2"></i>
                    <span>Lịch sử thay đổi</span>
                  </li>
                  <li
                    class="list-group-item"
                    :class="typeTab == 2 ? 'active_li' : ''"
                    @click="typeTab = 2"
                  >
                    <i class="pi pi-users mr-2"></i>
                    <span>Chia sẻ</span>
                  </li>
                </ul>
              </div>
            </div>
            <div class="col-12 field p-0" v-if="typeTab == 1">
              <div v-if="log_files.length > 0">
                <div
                  v-for="(item, index) in log_files"
                  :key="index"
                  class="col-12 flex p-1"
                  style="border-bottom: 1px solid #e9ecef"
                >
                  <div class="col-1 p-0 format-center">
                    <Avatar
                      v-bind:label="
                        item.avatar ? '' : item.last_name.substring(0, 1)
                      "
                      v-bind:image="basedomainURL + item.avatar"
                      style="background-color: #2196f3;
                        color: #ffffff;
                        width: 3rem;
                        height: 3rem;
                      "
                      :style="{
                        background: bgColor[index % 7],
                      }"
                      class="mr-2"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                  <div class="col-11 p-0">
                    <div class="pt-2 ml-2">
                      <div v-if="item.full_name" class="font-medium">
                        {{ item.full_name }}
                      </div>
                      <div
                        v-if="item.contents"
                        class="font-medium"
                        style="
                          display: -webkit-box;
                          -webkit-line-clamp: 2;
                          -webkit-box-orient: vertical;
                          overflow: hidden;
                          text-overflow: ellipsis;
                        "
                      >
                        <span v-html="item.contents"></span>
                      </div>
                      <div
                        class="fsz-xs flex w-full"
                        style="align-items: center"
                      >
                        <div class="text-sm" style="margin: 2px">
                          Thời gian:
                          {{
                            moment(new Date(item.created_date)).format(
                              "DD/MM/YYYY HH:mm"
                            )
                          }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div
                v-else
                class="
                  align-items-center
                  justify-content-center
                  p-4
                  text-center
                "
              >
                <img src="../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </div>
            <div class="col-12 field p-0" v-if="typeTab == 2">
              <div v-if="share_files && share_files.length > 0">
                <div
                  v-for="(item, index) in share_files"
                  :key="index"
                  class="col-12 flex p-1"
                  style="border-bottom: 1px solid #e9ecef"
                >
                  <div class="col-1 p-0 format-center">
                    <Avatar
                      v-if="item.last_name"
                      v-bind:label="
                        item.avatar ? '' : item.last_name.substring(0, 1)
                      "
                      v-bind:image="basedomainURL + item.avatar"
                      style="
                        background-color: #2196f3;
                        color: #ffffff;
                        width: 3rem;
                        height: 3rem;
                      "
                      :style="{
                        background: bgColor[index % 7],
                      }"
                      class="mr-2"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                  <div class="col-11 p-0">
                    <div class="pt-2 ml-2">
                      <div v-if="item.full_name" class="font-medium">
                        {{ item.full_name }}
                      </div>
                      <div v-if="item.organization_name">
                        Phòng ban: {{ item.organization_name }}
                      </div>
                      <div
                        class="fsz-xs flex w-full"
                        style="align-items: center"
                      >
                        <div class="text-sm" style="margin: 2px">
                          Ngày chia sẻ:
                          {{
                            item.created_date
                              ? moment(new Date(item.created_date)).format(
                                  "DD/MM/YYYY HH:mm"
                                )
                              : ""
                          }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div
                v-else
                class="
                  align-items-center
                  justify-content-center
                  p-4
                  text-center
                "
              >
                <img src="../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </div>
          </div>
        </div>
      </Sidebar>
      <Galleria
        :value="images"
        v-model:activeIndex="activeIndex"
        :responsiveOptions="responsiveOptions"
        :numVisible="7"
        containerStyle="max-width: 100vw;max-height:100vh"
        :circular="true"
        :fullScreen="true"
        :showItemNavigators="true"
        :showThumbnails="true"
        v-model:visible="displayAlbum"
      >
        <template #item="slotProps">
          <img
            :src="basedomainURL + slotProps.item.is_filepath"
            :alt="slotProps.item.file_name"
            style="max-width: 100%; max-height: 100%; display: block"
          />
        </template>
        <template #thumbnail="slotProps">
          <img
            :src="basedomainURL + slotProps.item.is_filepath"
            :alt="slotProps.item.file_name"
            height="80"
            style="display: block"
          />
        </template>
      </Galleria>
      <Menu
        id="overlay_More"
        ref="menuButMores"
        :model="itemButMores"
        :popup="true"
      />
      <Menu
        id="overlay_File"
        ref="menuButMoresFile"
        :model="itemButMoreFile"
        :popup="true"
      />
      <vue-simple-context-menu
        element-id="myFirstMenu"
        :options="checkoption(item_clicked)"
        ref="vueSimpleContextMenu1"
        @option-clicked="optionClicked"
      >
      </vue-simple-context-menu>
    </div>
  </div>
</template>
<style scoped>
@import url(./style_file.css);
@import "bootstrap/dist/css/bootstrap.min.css";
</style>
<style>
.menu_sort {
  min-width: fit-content !important;
}
.menu_sort .p-menuitem {
  padding: 5px 10px !important;
}

.menu_sort .p-menuitem:hover {
  cursor: pointer;
  background-color: #e9ecef;
}
.menu_sort .p-menuitem .active {
  color: #2196f3 !important;
}
.menu_sort .p-menuitem a {
  text-decoration: none;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-splitter) {
  .p-toolbar {
    background: none !important;
    border: none !important;
  }
  .p-treetable-wrapper {
    margin-top: 6px;
  }
  .p-treetable .p-treetable-thead > tr > th {
    background: #fff !important;
  }
}
::v-deep(.p-toolbar) {
  .p-toolbar-group-left {
    max-width: 92%;
    display: flex;
    flex-grow: 1 !important;
    flex-direction: column !important;
  }
}

::v-deep(.p-treetable) {
  .p-treetable-tbody > tr > td {
    padding: 0;
  }

  .p-treetable-thead > tr > th {
    padding: 1.4rem 1rem !important;
  }
}

::v-deep(.p-chip) {
  .p-chip-text {
    width: max-content !important;
  }
}

::v-deep(.col-12) {
  .p-inputswitch {
    top: 6px;
  }
}

::v-deep(.grid-item) {
  img {
    width: 100%;
    object-fit: contain;
  }
}

::v-deep(.p-card) {
  .p-card-body {
    padding: 0.5rem !important;
  }

  .p-card-title {
    margin-bottom: 0 !important;
  }
}

::v-deep(.vue-simple-context-menu) {
  .vue-simple-context-menu__item {
    min-width: 130px !important;
    font-size: 14px;
  }
}

::v-deep(.p-dataview) {
  .p-dataview-header {
    padding: 0 0 !important;
  }

  .p-breadcrumb {
    padding: 0 !important;
    border-left: 4px solid rgb(0 122 212) !important;
  }
}

::v-deep(.p-dataview-content) {
  .grid {
    display: grid !important;
  }
}

::v-deep(.grid-9) {
  .grid {
    grid-template-columns: repeat(9, 1fr);
  }
}

::v-deep(.grid-8) {
  .grid {
    grid-template-columns: repeat(8, 1fr);
  }
}

::v-deep(.grid-7) {
  .grid {
    grid-template-columns: repeat(7, 1fr);
  }
}

::v-deep(.grid-6) {
  .grid {
    grid-template-columns: repeat(6, 1fr);
  }
}

::v-deep(.grid-5) {
  .grid {
    grid-template-columns: repeat(5, 1fr);
  }
}

::v-deep(.grid-4) {
  .grid {
    grid-template-columns: repeat(4, 1fr);
  }
}

::v-deep(.grid-3) {
  .grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

::v-deep(.grid-2) {
  .grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>