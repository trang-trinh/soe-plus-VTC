<script setup>
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import DetailedWork from "../../components/task_origin/DetailedWork.vue";
import DialogTask from "../../components/task_origin/DialogTask.vue";
import moment from "moment";
import { encr } from "../../util/function.js";
import { useRoute } from "vue-router";
const cryoptojs = inject("cryptojs");
const router = inject("router");
const route = useRoute();
const basedomainURL = fileURL;
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const expandedKeys = ref([]);
document.onkeydown = fkey;
document.onkeypress = fkey;
document.onkeyup = fkey;

var wasPressed = false;

const props = defineProps({
  isShow: Boolean,
  id: String,
  turn: Intl,
});

function fkey(e) {
  if (idTaskLoaded.value != null) {
    e = e || window.event;
    if (wasPressed) return;

    if (e.keyCode == 116 || (e.keyCode == 65 && e.ctrlKey)) {
      wasPressed = true;
    } else {
      wasPressed = false;
    }
    if (wasPressed == true) {
      router.push({ name: "taskmain", params: {} }).then(() => {
        router.go(0);
      });
    }
  }
}

const expandedRowGroups = ref();
const checkDelList = ref(false);
const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const menuSortButs = ref();
const menuListTypeButs = ref();
const menuGroupListTypeButs = ref();
const menuFilterButs = ref();
const selectedTasks = ref();
const listTask = ref();
const displayTask = ref(false);
const headerAddTask = ref();
const listDropdownUser = ref([]);
const listDropdownorganization = ref([]);
const listOrganization = ref([]);
const listDropdownProject = ref([]);
const listDropdownTaskGroup = ref([]);
const first = ref(0);
const sttTask = ref();
const filterTime1 = ref();
const filterTime2 = ref();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
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
const opition = ref({
  IsNext: true,
  sort: "modified_date",
  ob: "DESC",
  PageNo: 0,
  PageSize: 20,
  search: "",
  Filteruser_id: null,
  user_id: store.getters.user_id,
  IsType: 0,
  SearchTextUser: "",
  filter_type: 0,
  sdate: null,
  edate: null,
  loctitle: "Lọc",
  type_view: 2,
  type_group_view: null,
  filter_date: null,
  filter_duan: null,
  filter_taskgroup: null,
});
const listDropdownStatus = ref([
  {
    value: 0,
    text: "Chưa bắt đầu",
    bg_color: "#bbbbbb",
    text_color: "#FFFFFF",
  },
  { value: 1, text: "Đang làm", bg_color: "#2196f3", text_color: "#FFFFFF" },
  { value: 2, text: "Tạm ngừng", bg_color: "#d87777", text_color: "#FFFFFF" },
  { value: 3, text: "Đã đóng", bg_color: "#d87777", text_color: "#FFFFFF" },
  { value: 4, text: "HT đúng hạn", bg_color: "#04D215", text_color: "#FFFFFF" },
  {
    value: 5,
    text: "Chờ đánh giá",
    bg_color: "#33c9dc",
    text_color: "#FFFFFF",
  },
  { value: 6, text: "Bị trả lại", bg_color: "#ffa500", text_color: "#FFFFFF" },
  { value: 7, text: "HT sau hạn", bg_color: "#ff8b4e", text_color: "#FFFFFF" },
  { value: 8, text: "Đã đánh giá", bg_color: "#51b7ae", text_color: "#FFFFFF" },
  { value: -1, text: "Bị xóa", bg_color: "red", text_color: "#FFFFFF" },
]);
// const listDropdownweight = ref([
//   { value: 0, text: "Rất dễ" },
//   { value: 1, text: "Dễ" },
//   { value: 2, text: "Bình thường" },
//   { value: 3, text: "Hơi khó" },
//   { value: 4, text: "Khó" },
//   { value: 5, text: "Rất khó" },
//   { value: 6, text: "Không khả thi" },
// ]);
const listDropdownweight = ref();
const rules = {
  task_name: {
    required,
    $errors: [
      {
        $property: "task_name",
        $validator: "required",
        $message: "Tên công việc không được để trống!",
      },
    ],
  },
  assign_user_id: {
    required,
    $errors: [
      {
        $property: "assign_user_id",
        $validator: "required",
        $message: "Người giao việc không được để trống!",
      },
    ],
  },
  work_user_ids: {
    required,
    $errors: [
      {
        $property: "work_user_ids",
        $validator: "required",
        $message: "Người thực hiện không được để trống!",
      },
    ],
  },
  end_date: {
    required,
    $errors: [
      {
        $property: "end_date",
        $validator: "required",
        $message: "Ngày kết thúc không được để trống!",
      },
    ],
  },
};

const itemSortButs = ref([
  {
    label: "Ngày cập nhật mới đến cũ",
    sort: "modified_date",
    ob: "DESC",
    active: true,
    command: () => {
      ChangeSortTask("modified_date", "DESC");
    },
  },
  {
    label: "Ngày cập nhật cũ đến mới",
    sort: "modified_date",
    ob: "ASC",
    active: false,
    command: () => {
      ChangeSortTask("modified_date", "ASC");
    },
  },
  {
    label: "Số thứ tự thấp đến cao",
    sort: "is_order",
    ob: "ASC",
    active: false,
    command: () => {
      ChangeSortTask("is_order", "ASC");
    },
  },
  {
    label: "Số thứ tự cao đến thấp",
    sort: "is_order",
    ob: "DESC",
    active: false,
    command: () => {
      ChangeSortTask("is_order", "DESC");
    },
  },
  {
    label: "Ngày tạo mới đến cũ",
    sort: "created_date",
    ob: "DESC",
    active: false,
    command: () => {
      ChangeSortTask("created_date", "DESC");
    },
  },
  {
    label: "Ngày tạo cũ đến mới",
    sort: "created_date",
    ob: "ASC",
    active: false,
    command: () => {
      ChangeSortTask("created_date", "ASC");
    },
  },
  {
    label: "Tên công việc A-Z",
    sort: "task_name",
    ob: "ASC",
    active: false,
    command: () => {
      ChangeSortTask("task_name", "ASC");
    },
  },
  {
    label: "Tên công việc Z-A",
    sort: "task_name",
    ob: "DESC",
    active: false,
    command: () => {
      ChangeSortTask("task_name", "DESC");
    },
  },
  {
    label: "Theo ngày nhận cũ đến mới",
    sort: "start_date",
    ob: "ASC",
    active: false,
    command: () => {
      ChangeSortTask("start_date", "ASC");
    },
  },
  {
    label: "Theo ngày nhận mới đến cũ",
    sort: "start_date",
    ob: "DESC",
    active: false,
    command: () => {
      ChangeSortTask("start_date", "DESC");
    },
  },
  {
    label: "Theo ngày hoàn thành cũ đến mới",
    sort: "finish_date",
    ob: "ASC",
    active: false,
    command: () => {
      ChangeSortTask("finish_date", "ASC");
    },
  },
  {
    label: "Theo ngày hoàn thành mới đến cũ",
    sort: "finish_date",
    ob: "DESC",
    active: false,
    command: () => {
      ChangeSortTask("finish_date", "DESC");
    },
  },
  // {
  //   label: "Số ngày quá hạn",
  // },
]);
const itemGroupListTypeButs = ref([
  {
    label: "Dự án",
    active: false,
    icon: "pi pi-briefcase",
    type: 1,
    command: () => {
      ChangeGroupView(1);
    },
  },
  {
    label: "Nhóm công việc",
    active: false,
    icon: "pi pi-clone",
    type: 2,
    command: () => {
      ChangeGroupView(2);
    },
  },
]);
const itemListTypeButs = ref([
  {
    label: "LIST",
    active: false,
    icon: "pi pi-list",
    type: 1,
    command: () => {
      ChangeView(1);
    },
  },
  {
    label: "TREE",
    active: true,
    icon: "pi pi-list",
    type: 2,
    command: () => {
      ChangeView(2);
    },
  },
  {
    label: "GRID",
    active: false,
    icon: "pi pi-table",
    type: 3,
    command: () => {
      ChangeView(3);
    },
  },
  {
    label: "GANTT",
    active: false,
    icon: "pi pi-calendar-plus",
    type: 4,
    command: () => {
      ChangeView(4);
    },
  },
  {
    label: "USER",
    active: false,
    icon: "pi pi-user-plus",
    type: 5,
    command: () => {
      ChangeView(5);
    },
  },
]);

const ChangeGroupView = (data) => {
  type_group_view_refresh.value = data;
  menuGroupListTypeButs.value.toggle();
  loadData(true, opition.value.type_view);
  itemGroupListTypeButs.value.forEach((t) => {
    if (data != t.type) {
      t.active = false;
    } else {
      t.active = true;
    }
  });
};

const ChangeView = (data) => {
  if (data.type == 3) {
    opition.value.PageSize = 10000;
  } else {
    opition.value.PageSize = 20;
  }
  loadData(true, data.type);
  itemListTypeButs.value.forEach((t) => {
    if (data.type != t.type) {
      t.active = false;
    } else {
      t.active = true;
    }
  });
  menuListTypeButs.value.toggle();
};
const itemFilterButs = ref([
  {
    label: "",
    icon: "",
    active: false,
    istype: 5,
    hasChildren: true,
    groups: [
      {
        label: "Theo ngày nhận",
        icon: "pi pi-calendar",
        active: false,
        is_children: 1,
        filter_date: new Date(),
      },
      {
        label: "Dự án",
        icon: "pi pi-calendar",
        active: false,
        is_children: 2,
      },
    ],
  },
  {
    label: "",
    icon: "",
    active: false,
    istype: 6,
    hasChildren: true,
    groups: [
      {
        label: "Ngày hoàn thành",
        icon: "pi pi-calendar",
        active: false,
        is_children: 3,
        filter_date: new Date(),
      },
      {
        label: "Nhóm công việc",
        icon: "pi pi-calendar",
        active: false,
        is_children: 4,
      },
    ],
  },
  // {
  //   label: "Theo ngày nhận",
  //   icon: "pi pi-calendar",
  //   active: false,
  //   istype: 5,
  //   filter_date: new Date(),
  // },
  // {
  //   label: "Ngày hoàn thành",
  //   icon: "pi pi-calendar",
  //   active: false,
  //   istype: 6,
  //   filter_date: new Date(),
  // },
  {
    label: "Trong tuần",
    icon: "pi pi-calendar",
    active: false,
    istype: 1,
    hasChildren: false,
  },
  {
    label: "Trong tháng",
    icon: "pi pi-calendar",
    active: false,
    istype: 2,
    hasChildren: false,
  },
  {
    label: "Trong năm",
    icon: "pi pi-calendar",
    active: false,
    istype: 3,
    hasChildren: false,
  },
  {
    label: "Theo thời gian",
    icon: "pi pi-calendar-times",
    active: false,
    istype: 4,
    hasChildren: true,
    groups: [
      {
        label: "Ngày bắt đầu",
        icon: "pi pi-calendar",
        children_id: true,
        is_change: 1,
      },
      {
        label: "Ngày kết thúc",
        icon: "pi pi-calendar",
        children_id: true,
        is_change: 2,
      },
    ],
  },
]);
const listDropdownUserAssign = ref([]);
const emitter = inject("emitter");
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "listDropdownUserAssign":
      listDropdownUserAssign.value = obj.data;
      break;
  }
});
const toggleSort = (event) => {
  menuSortButs.value.toggle(event);
};
const toggleFilter = (event) => {
  menuFilterButs.value.toggle(event);
};
const toggleListType = (event) => {
  menuListTypeButs.value.toggle(event);
};
const toggleGroupListType = (event) => {
  menuGroupListTypeButs.value.toggle(event);
};
const ChangeSortTask = (sort, ob) => {
  opition.value.sort = sort;
  opition.value.ob = ob;
  itemSortButs.value.forEach((i) => {
    if (i.sort == sort && i.ob == ob) {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  menuSortButs.value.toggle();
  loadData(true, opition.value.type_view);
};
const ChangeTimeFilter = (type, value) => {
  opition.value.filter_type = 4;
  itemFilterButs.value.forEach((i) => {
    if (i.istype == 4) {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  if (type == 1) {
    filterTime1.value = moment(new Date(value)).format("DD/MM/YYYY HH:mm");
    opition.value.sdate = value;
  } else {
    filterTime2.value = moment(new Date(value)).format("DD/MM/YYYY HH:mm");
    opition.value.edate = value;
  }
};
const removeTime = (type) => {
  if (type == 1) {
    filterTime1.value = null;
    opition.value.sdate = null;
  } else {
    filterTime2.value = null;
    opition.value.edate = null;
  }
  if (opition.value.sdate) {
    if (opition.value.edate) {
      opition.value.loctitle =
        "Từ " +
        moment(opition.value.sdate).format("DD/MM/YYYY") +
        " - " +
        moment(opition.value.edate).format("DD/MM/YYYY");
    } else {
      opition.value.loctitle =
        "Từ ngày " + moment(opition.value.sdate).format("DD/MM/YYYY");
    }
  } else {
    if (opition.value.edate) {
      opition.value.loctitle =
        "Đến ngày " + moment(opition.value.edate).format("DD/MM/YYYY");
    } else {
      opition.value.loctitle = "Lọc";
    }
  }
  if (filterTime1.value == null && filterTime2.value == null) {
    itemFilterButs.value.forEach((i) => {
      if (i.istype == 4) {
        i.active = false;
      }
    });
  }
};

const ChangeFilterAdvanced = (type) => {
  if (type == 2) {
    opition.value.filter_taskgroup = null;
    opition.value.loctitle = "Theo dự án";
  } else {
    opition.value.filter_duan = null;
    opition.value.loctitle = "Theo nhóm công việc";
  }
  opition.value.sdate = null;
  filterTime1.value = null;
  opition.value.edate = null;
  filterTime2.value = null;
  menuFilterButs.value.toggle();
  loadData(true, opition.value.type_view);
};

const ChangeFilter = (type, act) => {
  opition.value.filter_type = type;
  opition.value.filter_duan = null;
  opition.value.filter_taskgroup = null;
  itemFilterButs.value.forEach((i) => {
    if (i.istype == type) {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  var date = new Date();
  switch (type) {
    case -1: //tất cả
      opition.value.sdate = null;
      filterTime1.value = null;
      opition.value.edate = null;
      filterTime2.value = null;
      opition.value.loctitle = "Lọc";
      break;
    case 1: //Trong tuần
      opition.value.sdate = moment().startOf("isoWeek").toDate();
      opition.value.edate = moment().endOf("isoWeek").toDate();
      opition.value.loctitle = "Trong tuần";
      break;
    case 5: //theo ngày nhận
      opition.value.sdate = null;
      opition.value.edate = null;

      itemFilterButs.value
        .filter((x) => x.istype == 5)
        .forEach((t) => {
          t.groups
            .filter((y) => y.is_children == 1)
            .forEach((d) => {
              d.label =
                "Theo ngày nhận" +
                " (" +
                moment(t.filter_date).format("DD/MM/YYYY HH:mm") +
                ")";
              opition.value.filter_date = t.filter_date;
            });
        });

      itemFilterButs.value
        .filter((x) => x.istype == 6)
        .forEach((t) => {
          t.groups
            .filter((y) => y.is_children == 3)
            .forEach((d) => {
              d.label = "Ngày hoàn thành";
            });
        });
      opition.value.loctitle = "Theo ngày nhận";
      break;
    case 6: //theo ngày hoàn thành
      opition.value.sdate = null;
      opition.value.edate = null;
      itemFilterButs.value
        .filter((x) => x.istype == 6)
        .forEach((t) => {
          t.groups
            .filter((y) => y.is_children == 3)
            .forEach((d) => {
              d.label =
                "Ngày hoàn thành (" +
                moment(t.filter_date).format("DD/MM/YYYY HH:mm") +
                ")";
            });
          opition.value.filter_date = t.filter_date;
        });
      itemFilterButs.value
        .filter((x) => x.istype == 5)
        .forEach((t) => {
          t.groups
            .filter((y) => y.is_children == 1)
            .forEach((d) => {
              d.label = "Theo ngày nhận";
            });
        });
      opition.value.loctitle = "Theo ngày hoàn thành";
      break;
    case 2: //Trong tháng
      opition.value.sdate = new Date(date.getFullYear(), date.getMonth(), 1);
      opition.value.edate = new Date(
        date.getFullYear(),
        date.getMonth() + 1,
        0,
      );
      opition.value.loctitle = "Trong tháng";
      break;
    case 3: //Trong năm
      opition.value.sdate = new Date(date.getFullYear(), 1, 1);
      opition.value.edate = new Date(date.getFullYear(), 12, 31);
      opition.value.loctitle = "Trong năm";
      break;
    default:
      if (opition.value.sdate) {
        if (opition.value.edate) {
          opition.value.loctitle =
            "Từ " +
            moment(opition.value.sdate).format("DD/MM/YYYY") +
            " - " +
            moment(opition.value.edate).format("DD/MM/YYYY");
        } else {
          opition.value.loctitle =
            "Từ ngày " + moment(opition.value.sdate).format("DD/MM/YYYY");
        }
      } else {
        if (opition.value.edate) {
          opition.value.loctitle =
            "Đến ngày " + moment(opition.value.edate).format("DD/MM/YYYY");
        }
      }
      break;
  }
  filterTime1.value = opition.value.sdate
    ? moment(new Date(opition.value.sdate)).format("DD/MM/YYYY")
    : null;
  filterTime2.value = opition.value.edate
    ? moment(new Date(opition.value.edate)).format("DD/MM/YYYY")
    : null;
  if (type == 1 || type == 2 || type == 3) {
    menuFilterButs.value.toggle();
    loadData(true, opition.value.type_view);
  } else {
    if (act == true) {
      menuFilterButs.value.toggle();
      loadData(true, opition.value.type_view);
    }
  }
};
const Del_ChangeFilter = () => {
  opition.value.filter_duan = null;
  opition.value.filter_taskgroup = null;
  opition.value.filter_type = 0;
  filterTime1.value = null;
  filterTime2.value = null;
  opition.value.sdate = null;
  opition.value.edate = null;
  opition.value.filter_date = null;
  opition.value.loctitle = "Lọc";
  // itemFilterButs.value.forEach((i) => {
  //   if(i.istype == 5 || i.istype == 6){
  //     i.filter_date = new Date();
  //   }
  //   i.active = false;
  // });
  itemFilterButs.value = [
    {
      label: "",
      icon: "",
      active: false,
      istype: 5,
      hasChildren: true,
      groups: [
        {
          label: "Theo ngày nhận",
          icon: "pi pi-calendar",
          active: false,
          is_children: 1,
          filter_date: new Date(),
        },
        {
          label: "Dự án",
          icon: "pi pi-calendar",
          active: false,
          is_children: 2,
        },
      ],
    },
    {
      label: "",
      icon: "",
      active: false,
      istype: 6,
      hasChildren: true,
      groups: [
        {
          label: "Ngày hoàn thành",
          icon: "pi pi-calendar",
          active: false,
          is_children: 3,
          filter_date: new Date(),
        },
        {
          label: "Nhóm công việc",
          icon: "pi pi-calendar",
          active: false,
          is_children: 4,
        },
      ],
    },
    // {
    //   label: "Theo ngày nhận",
    //   icon: "pi pi-calendar",
    //   active: false,
    //   istype: 5,
    //   filter_date: new Date(),
    // },
    // {
    //   label: "Ngày hoàn thành",
    //   icon: "pi pi-calendar",
    //   active: false,
    //   istype: 6,
    //   filter_date: new Date(),
    // },
    {
      label: "Trong tuần",
      icon: "pi pi-calendar",
      active: false,
      istype: 1,
      hasChildren: false,
    },
    {
      label: "Trong tháng",
      icon: "pi pi-calendar",
      active: false,
      istype: 2,
      hasChildren: false,
    },
    {
      label: "Trong năm",
      icon: "pi pi-calendar",
      active: false,
      istype: 3,
      hasChildren: false,
    },
    {
      label: "Theo thời gian",
      icon: "pi pi-calendar-times",
      active: false,
      istype: 4,
      hasChildren: true,
      groups: [
        {
          label: "Ngày bắt đầu",
          icon: "pi pi-calendar",
          children_id: true,
          is_change: 1,
        },
        {
          label: "Ngày kết thúc",
          icon: "pi pi-calendar",
          children_id: true,
          is_change: 2,
        },
      ],
    },
  ];
  menuFilterButs.value.toggle();
  loadData(true, opition.value.type_view);
};
watch(selectedTasks, () => {
  if (selectedTasks.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});

const is_Add = ref(false);
const is_Template = ref(false);
const addTask = (str) => {
  headerAddTask.value = str;
  displayTask.value = true;
  is_Add.value = true;
};
const closeDialogTask = () => {
  displayTask.value = false;
};
const afterSave = () => {
  loadData(false, opition.value.type_view);
};
const DialogData = ref();
const editTask = (task_data) => {
  headerAddTask.value = "Sửa công việc";
  displayTask.value = true;
  DialogData.value = task_data;
  is_Add.value = false;
};
const DelTask = (dataTask) => {
  if (dataTask.childtasks > 0) {
    swal.fire({
      title: "Thông báo",
      html: "Tồn tại công việc con bạn không thể xóa!",
      icon: "info",
      confirmButtonText: "OK",
    });
  } else {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá công việc này không!",
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
          var listId = [];
          if (!dataTask) {
            selectedTasks.value.forEach(function (pg) {
              listId.push(pg.task_id);
            });
          }
          axios
            .delete(baseURL + "/api/task_origin/Delete_task_origin", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: dataTask != null ? [dataTask.task_id] : listId,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá công việc thành công!");
                loadData(true, opition.value.type_view);
              } else {
                swal.fire({
                  title: "Thông báo!",
                  html: response.data.ms,
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            })
            .catch((error) => {
              swal.close();
              if (error.status === 401) {
                swal.fire({
                  title: "Thông báo!",
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            });
        }
      });
  }
};
const interval = ref(null);
const startProgress = (datalists) => {
  interval.value = setInterval(() => {
    let newValue = Math.floor(Math.random() * 10) + 1;
    if (newValue < datalists) {
      newValue = datalists;
    }
    datalists = newValue;
  }, 5000);
};
const endProgress = () => {
  clearInterval(interval.value);
  interval.value = null;
};

onBeforeUnmount(() => {
  endProgress();
});

const ChangeData = (type) => {
  opition.value.IsType = type;
  opition.value.PageNo = 0;
  if (opition.value.type_view == 3 || opition.value.type_view == 2) {
    opition.value.PageSize = 10000;
  } else {
    opition.value.PageSize = 20;
  }
  first.value = 0;
  loadData(true, opition.value.type_view);
};
const RenderData = (data) => {
  // listTask.value = [];
  // opition.value.totalRecords = null;
  let arrChils = [];
  data
    .filter(
      (x) =>
        x.parent_id == null ||
        (x.parent_id != null &&
          data.filter((y) => y.task_id == x.parent_id).length == 0),
    )
    .forEach((m, i) => {
      m.STT2 = opition.value.PageNo * opition.value.PageSize + i + 1;
      let om = { key: m.task_id, data: m };
      const rechildren = (mm, task_id) => {
        let dts = data.filter((x) => x.parent_id == task_id);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, j) => {
            em.STT2 = mm.data.STT2 + "." + (j + 1);
            let om1 = { key: em.task_id, data: em };
            om1.data.is_order = j + 1;
            rechildren(om1, em.task_id);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m.task_id);

      arrChils.push(om);
    });
  return arrChils;
};

const groupBy = (list, props) => {
  return list.reduce((a, b) => {
    (a[b[props]] = a[b[props]] || []).push(b);
    return a;
  }, {});
};

const loadData = (rf, type) => {
  if (type == 3 || type == 2 || type == 4 || type == 5) {
    opition.value.PageSize = 100000;
  }
  if (rf) {
    opition.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_new",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
              { par: "sort", va: opition.value.sort },
              { par: "ob", va: opition.value.ob },
              { par: "IsType", va: opition.value.IsType },
              { par: "loc", va: opition.value.filter_type },
              { par: "sdate", va: opition.value.sdate },
              { par: "edate", va: opition.value.edate },
              { par: "filter_date", va: opition.value.filter_date },
              { par: "filter_duan", va: opition.value.filter_duan },
              { par: "filter_taskgroup", va: opition.value.filter_taskgroup },
              { par: "project_id", va: props.id ? props.id : null },
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
      let data1 = JSON.parse(response.data.data)[0];
      if (data1.length > 0) {
        data1.forEach((element, i) => {
          element.progress = element.progress == null ? 0 : element.progress;
          element.update_date = element.modified_date
            ? element.modified_date
            : element.created_date;
          element.status_name =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text
              : "";

          element.status_name =
            element.count_extend > 0 ? "Xin gia hạn" : element.status_name;
          element.status_bg_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].bg_color
              : "";
          element.status_bg_color =
            element.count_extend > 0 ? "#F18636" : element.status_bg_color;
          element.status_text_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text_color
              : "";
          //thời gian xử lý
          if (element.end_date != null) {
            if (element.thoigianquahan <= 0) {
              if (element.thoigianxuly > 0 && element.thoigianxuly < 60) {
                element.title_time = element.thoigianxuly + " phút";
              } else if (
                element.thoigianxuly >= 60 &&
                element.thoigianxuly < 1440
              ) {
                element.title_time =
                  Math.floor(element.thoigianxuly / 60) + " giờ";
              } else {
                element.title_time =
                  Math.floor(element.thoigianxuly / 1440) + " ngày";
              }
              element.totalDay = element.thoigianxuly;
              element.time_bg = element.status_bg_color;
              element.time_color = "color: #fff;";
            } else {
              if (element.thoigianquahan > 0) {
                if (element.thoigianquahan > 0 && element.thoigianquahan < 60) {
                  element.title_time =
                    "Quá hạn " + element.thoigianquahan + " phút";
                } else if (
                  element.thoigianquahan >= 60 &&
                  element.thoigianquahan < 1440
                ) {
                  element.title_time =
                    "Quá hạn " +
                    Math.floor(element.thoigianquahan / 60) +
                    " giờ";
                } else {
                  element.title_time =
                    "Quá hạn " +
                    Math.floor(element.thoigianquahan / 1440) +
                    " ngày";
                }
                // element.title_time = "Quá hạn " + element.thoigianquahan + " ngày";
                element.totalDay = element.thoigianquahan;
                element.time_bg = "red";
                element.time_color = "color: #fff;";
              }
            }
          } else if (element.thoigianxuly) {
            if (element.thoigianxuly > 0 && element.thoigianxuly < 60) {
              element.title_time = element.thoigianxuly + " phút";
            } else if (
              element.thoigianxuly >= 60 &&
              element.thoigianxuly < 1440
            ) {
              element.title_time =
                Math.floor(element.thoigianxuly / 60) + " giờ";
            } else {
              element.title_time =
                Math.floor(element.thoigianxuly / 1440) + " ngày";
            }
            // element.title_time = element.thoigianxuly + " ngày";
            element.totalDay = element.thoigianxuly;
            element.time_bg = element.status_bg_color;
            element.time_color = "color: #fff;";
          }

          element.Thanhviens = element.Thanhviens
            ? JSON.parse(element.Thanhviens)
            : [];
          element.ThanhvienShows = [];
          if (element.Thanhviens.length > 3) {
            element.ThanhvienShows = element.Thanhviens.slice(0, 3);
          } else {
            element.ThanhvienShows = [...element.Thanhviens];
          }
          element.files = element.files ? JSON.parse(element.files) : [];
          element.files = element.files ? element.files : [];
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
          startProgress(element.progress);
        });
      }
      opition.value.totalRecords = data[8][0].totalRecords;
      opition.value.totalAlls = data[1][0].total;
      opition.value.total_toilam = data[2][0].total_toilam;
      opition.value.total_quanly = data[3][0].total_quanly;
      opition.value.total_theodoi = data[4][0].total_theodoi;
      opition.value.total_toitao = data[5][0].total_toitao;
      opition.value.total_hoanthanh = data[6][0].total_hoanthanh;
      opition.value.total_quahan = data[7][0].total_quahan;
      opition.value.type_view = type;
      opition.value.type_group_view = type_group_view_refresh.value;
      if (type == 1) {
        listTask.value = data1;
        sttTask.value = data[1][0].total + 1;
      }
      if (type == 2) {
        if (opition.value.type_group_view) {
          var arrNew = [];
          if (opition.value.type_group_view == 1) {
            var listCV = groupBy(data1, "project_id");
            for (let k in listCV) {
              var CVGroup = [];
              listCV[k].forEach(function (r) {
                CVGroup.push(r);
              });
              arrNew.push({
                isShow: true,
                status: k,
                group_view_name:
                  k == "null"
                    ? ""
                    : listDropdownProject.value.filter(
                        (x) => x.project_id == k,
                      )[0].project_name,
                group_view_bg_color: "#0d89ec",
                CVGroup: RenderData(CVGroup),
              });
            }
          } else {
            listCV = groupBy(data1, "group_id");
            for (let k in listCV) {
              CVGroup = [];
              listCV[k].forEach(function (r) {
                CVGroup.push(r);
              });
              arrNew.push({
                isShow: true,
                status: k,
                group_view_name:
                  k == "null"
                    ? ""
                    : listDropdownTaskGroup.value.filter(
                        (x) => x.group_id == k,
                      )[0].group_name,
                group_view_bg_color: "#2196f3 ",
                CVGroup: RenderData(CVGroup),
              });
            }
          }
          listTask.value = arrNew;
        } else {
          listTask.value = RenderData(data1);
        }
      }
      if (type == 3) {
        if (opition.value.type_group_view) {
          if (opition.value.type_group_view == 1) {
            listCV = groupBy(data1, "project_id");
            arrNew = [];
            for (let k in listCV) {
              CVGroup = [];
              listCV[k].forEach(function (r) {
                CVGroup.push(r);
              });
              arrNew.push({
                status: k,
                group_view_name:
                  k == "null"
                    ? ""
                    : listDropdownProject.value.filter(
                        (x) => x.project_id == k,
                      )[0].project_name,
                group_view_bg_color: "#0d89ec",
                CVGroup: CVGroup,
              });
            }
            listTask.value = arrNew;
            listTask.value.forEach(function (t) {
              if (t.CVGroup.length > 0) {
                let listCV = groupBy(t.CVGroup, "status");
                t.ListCVGroup = [];
                for (let k in listCV) {
                  var CVGroup2 = [];
                  listCV[k].forEach(function (r) {
                    CVGroup2.push(r);
                  });
                  t.ListCVGroup.push({
                    isShow: false,
                    status: k,
                    group_view_name: listDropdownStatus.value.filter(
                      (x) => x.value == k,
                    )[0].text,
                    group_view_bg_color: listDropdownStatus.value.filter(
                      (x) => x.value == k,
                    )[0].bg_color,
                    CVGroup2: CVGroup2,
                  });
                }
              }
            });
            sttTask.value = data[1][0].total + 1;
          } else {
            listCV = groupBy(data1, "group_id");
            arrNew = [];
            for (let k in listCV) {
              CVGroup = [];
              listCV[k].forEach(function (r) {
                CVGroup.push(r);
              });
              arrNew.push({
                status: k,
                group_view_name:
                  k == "null"
                    ? ""
                    : listDropdownTaskGroup.value.filter(
                        (x) => x.group_id == k,
                      )[0].group_name,
                group_view_bg_color: "#2196f3 ",
                CVGroup: CVGroup,
              });
            }
            listTask.value = arrNew;
            listTask.value.forEach(function (t) {
              if (t.CVGroup.length > 0) {
                let listCV = groupBy(t.CVGroup, "status");
                t.ListCVGroup = [];
                for (let k in listCV) {
                  var CVGroup2 = [];
                  listCV[k].forEach(function (r) {
                    CVGroup2.push(r);
                  });
                  t.ListCVGroup.push({
                    isShow: false,
                    group_view_name: listDropdownStatus.value.filter(
                      (x) => x.value == k,
                    )[0].text,
                    group_view_bg_color: listDropdownStatus.value.filter(
                      (x) => x.value == k,
                    )[0].bg_color,
                    CVGroup2: CVGroup2,
                  });
                }
              }
            });
          }
        } else {
          listCV = groupBy(data1, "status");
          arrNew = [];
          for (let k in listCV) {
            CVGroup = [];
            listCV[k].forEach(function (r) {
              CVGroup.push(r);
            });
            arrNew.push({
              status: k,
              group_view_name: listDropdownStatus.value.filter(
                (x) => x.value == k,
              )[0].text,
              group_view_bg_color: listDropdownStatus.value.filter(
                (x) => x.value == k,
              )[0].bg_color,
              CVGroup: CVGroup,
            });
          }
          listTask.value = arrNew;
          sttTask.value = data[1][0].total + 1;
        }
      }
      if (type == 4 || type == 5) {
        listTask.value = data1;
        let date1 = new Date(
          opition.value.sdate ? opition.value.sdate : new Date(),
        );
        let date2 = new Date(
          opition.value.edate ? opition.value.edate : new Date(),
        );
        // var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
        // var lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0);
        var firstDay = new Date(date1.getFullYear(), date1.getMonth(), 1);
        var lastDay = new Date(date2.getFullYear(), date2.getMonth() + 1, 0);
        getDates(firstDay, lastDay);
      }
      if (idTaskLoaded.value != null) {
        showDetail.value = false;
        showDetail.value = true;
        selectedTaskID.value = idTaskLoaded.value;
      }
      if (rf) {
        opition.value.loading = false;
        swal.close();
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!" + error);
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
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

const listThanhVien = ref();

const listUser = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_task_origin",
            par: [
              { par: "search", va: opition.value.SearchTextUser },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "role_id", va: null },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "department_id", va: null },
              { par: "position_id", va: null },

              { par: "isadmin", va: null },
              { par: "status", va: null },
              { par: "start_date", va: null },
              { par: "end_date", va: null },
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
      listDropdownUser.value = data.map((x) => ({
        name: x.full_name,
        code: x.user_id,
        avatar: x.avatar,
        ten: x.last_name,
      }));
      if (listDropdownUser.value.length > 10) {
        listThanhVien.value = listDropdownUser.value.slice(0, 10);
      } else {
        listThanhVien.value = [...listDropdownUser.value];
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const renderTreeDV = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  // .filter((x) => x.parent_id == null)
  data.forEach((m, i) => {
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
    label: "-----Chọn " + title + "----",
  });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const listtreeOrganization = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_pb",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let obj = renderTreeDV(
        data.filter((x) => x.organization_type != 0),
        "organization_id",
        "organization_name",
        "phòng ban",
      );
      listOrganization.value = data;
      listDropdownorganization.value = obj.arrtreeChils;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const listProjectMain = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_get_list_init",
            par: [
              {
                par: "user_id",
                va: store.getters.user.user_id,
              },
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
      listDropdownProject.value = data[0];
      listDropdownTaskGroup.value = data[1];
      listDropdownweight.value = data[2];
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const onPage = (event) => {
  if (event.rows != opition.value.PageSize) {
    opition.value.PageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    opition.value.id = null;
    opition.value.IsNext = true;
  } else if (event.page > opition.value.PageNo + 1) {
    //Trang cuối
    opition.value.id = -1;
    opition.value.IsNext = false;
  } else if (event.page > opition.value.PageNo) {
    //Trang sau

    opition.value.id = listTask.value[listTask.value.length - 1].task_id;
    opition.value.IsNext = true;
  } else if (event.page < opition.value.PageNo) {
    //Trang trước
    opition.value.id = listTask.value[0].task_id;
    opition.value.IsNext = false;
  }
  opition.value.PageNo = event.page;
  loadData(true, opition.value.type_view);
};
const type_group_view_refresh = ref();
const onRefresh = () => {
  type_group_view_refresh.value = null;
  opition.value = {
    IsNext: true,
    sort: "modified_date",
    ob: "DESC",
    PageNo: 0,
    PageSize: 20,
    search: "",
    Filteruser_id: null,
    user_id: store.getters.user_id,
    IsType: 0,
    SearchTextUser: "",
    filter_type: 0,
    sdate: null,
    edate: null,
    loctitle: "Lọc",
    type_view: 2,
    filter_date: null,
    type_group_view: opition.value.type_group_view,
    filter_duan: null,
    filter_taskgroup: null,
  };
  itemSortButs.value.forEach((i) => {
    if (i.sort == opition.value.sort && i.ob == opition.value.ob) {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  first.value = 0;
  filterTime1.value = null;
  filterTime2.value = null;
  itemFilterButs.value.forEach((i) => {
    i.active = false;
  });
  loadData(true, opition.value.type_view);
};
const idTaskLoaded = ref(route.params.id);

function getDaysInMonth(year, month) {
  return new Date(year, month, 0).getDate();
}

const getDates = (startDate, endDate) => {
  var dateArray = [];
  var currentDate = moment(startDate);
  var stopDate = moment(endDate);
  while (currentDate <= stopDate && currentDate) {
    var d = moment.utc(currentDate).toDate();
    var date = new Date();
    var currentYear = date.getFullYear();
    var currentMonth = date.getMonth() + 1;
    dateArray.push({
      DayN: moment(currentDate).format("DD"),
      DW: d.getDay(),
      Day: parseInt(moment(currentDate).format("DD")),
      DayName: WeekDay.value.filter(
        (x) => x.value == d.toLocaleString("en-us", { weekday: "long" }),
      )[0].text,
      bg: WeekDay.value.filter(
        (x) => x.value == d.toLocaleString("en-us", { weekday: "long" }),
      )[0].bg,
      color:
        parseInt(moment(currentDate).format("DD")) ==
        parseInt(moment(new Date()).format("DD"))
          ? "#ff0000"
          : "",
      totalDayCurrent: getDaysInMonth(currentYear, currentMonth),
      currentDate: currentDate,
      Month: d.getMonth(),
      Year: d.getFullYear(),
    });
    currentDate = moment(currentDate).add(1, "days");
  }
  listTask.value.forEach(function (d) {
    var dates = [];
    var bd = new Date(d.start_date);
    bd.setHours(0, 0, 0, 0);
    dateArray.forEach(function (t, i) {
      var to = { DW: t.DW, Day: t.Day, totalDay: 0 };
      if (
        new Date(t.currentDate) >= bd &&
        new Date(t.currentDate) <=
          new Date(d.finish_date != null ? d.finish_date : new Date())
      ) {
        to.IsCheck = true;
        to.Name = d.task_name;
        to.totalDay = to.totalDay + 1;
        if (i > 0 && dates[i - 1].IsCheck) {
          to.IsHide = true;
        } else {
          to.IsHide = false;
        }
      } else {
        to.IsHide = false;
      }
      to.color = t.color;
      to.bg = t.bg;
      dates.push(to);
    });
    d.totalDay = dates.filter((x) => x.IsCheck == true).length;
    d.dateArray = dates.filter((x) => x.IsHide == false);
  });
  GrandsDate.value = dateArray;

  if (opition.value.type_view == 5) {
    var listData = [];
    listTask.value.forEach(function (cv) {
      cv.Thanhviens.forEach(function (u) {
        if (
          listData.filter(
            (x) => x.user_id == u.user_id && x.task_id == cv.task_id,
          ).length == 0
        ) {
          listData.push({
            user_id: u.user_id,
            task_id: cv.task_id,
            user_name: u.fullName,
            dateArray: cv.dateArray,
            totalDay: cv.totalDay,
            time_bg: cv.status_bg_color,
            status_text_color: cv.status_text_color,
            avatar: u.avatar,
            last_name: u.ten,
            tenToChuc: u.tenToChuc,
            tenChucVu: u.tenChucVu,
            is_type: parseInt(u.is_type),
          });
        }
      });
    });
    let listCV = groupBy(listData, "user_id");
    var arrNew = [];
    for (let k in listCV) {
      listCV[k].forEach(function (r, i) {
        r.IsHienThi = i == 0 ? true : false;
        r.count_cv = listCV[k].length;
        r.count_istype_0 = listCV[k].filter((x) => x.is_type == 0).length;
        r.count_istype_1 = listCV[k].filter(
          (x) => x.is_type == 1 || x.is_type == 2,
        ).length;
        r.count_istype_3 = listCV[k].filter((x) => x.is_type == 3).length;
        arrNew.push(r);
      });
    }
    listTask.value = arrNew;
  }

  var years = [];
  for (
    var i = new Date(startDate).getFullYear();
    i <= new Date(endDate).getFullYear();
    i++
  ) {
    for (var j = 0; j < 12; j++) {
      var Month = { Month: j + 1, Year: i, Dates: [] };
      Month.Dates = dateArray.filter((x) => x.Month === j && x.Year === i);
      if (Month.Dates.length > 0) years.push(Month);
    }
  }
  Grands.value = years;
};

const GrandsDate = ref();
const Grands = ref();
const WeekDay = ref([
  { value: "Monday", text: "T2", bg: "" },
  { value: "Tuesday", text: "T3", bg: "" },
  { value: "Wednesday", text: "T4", bg: "" },
  { value: "Thursday", text: "T5", bg: "" },
  { value: "Friday", text: "T6", bg: "" },
  { value: "Saturday", text: "T7", bg: "aliceblue" },
  { value: "Sunday", text: "CN", bg: "antiquewhite" },
]);
const typefRouter = ref(route.params.type);
onMounted(() => {
  loadData(true, 2);
  if (typefRouter.value != null) {
    ChangeData(typefRouter.value);
  }
  listUser();
  listtreeOrganization();
  listProjectMain();
  startProgress();
  return {};
});

const showDetail = ref(false);
const selectedTaskID = ref();
const onRowSelect = (id) => {
  forceRerender();
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id.task_id;
};
const selectedKeys = ref();
const onNodeSelect = (id) => {
  forceRerender();
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id.data.task_id;
};
const closeDetail = () => {
  showDetail.value = false;
  selectedTaskID.value = null;
  loadData(false, opition.value.type_view);
};

const ChangeShowListCVGroup = (model) => {
  model.isShow = !model.isShow;
};
</script>
<template>
  <div
    v-if="store.getters.islogin"
    class="main-layout true flex-grow-1 p-2"
  >
    <div class="flex justify-content-center align-items-center">
      <Toolbar class="w-full custoolbar">
        <template #start>
          <span class="p-input-icon-left">
            <i class="pi pi-search" />
            <InputText
              style="min-width: 300px"
              type="text"
              spellcheck="false"
              v-model="opition.search"
              placeholder="Tìm kiếm"
              @keyup.enter="loadData(true, opition.type_view)"
            />
          </span>
        </template>

        <template #end>
          <Button
            @click="addTask('Tạo công việc')"
            label="Tạo công việc"
            icon="pi pi-plus"
            class="mr-2"
          />
          <ul
            id="toolbar_right"
            style="padding: 0px; margin: 0px; display: flex"
          >
            <li
              @click="toggleListType"
              aria-haspopup="true"
              :class="{ active: opition.type_view != 0 }"
              aria-controls="overlay_Export1"
            >
              <a
                ><i
                  style="margin-right: 5px"
                  class="pi pi-bars"
                ></i
                >Kiểu hiển thị<i
                  style="margin-left: 5px"
                  class="pi pi-angle-down"
                ></i
              ></a>
            </li>
            <li
              @click="toggleGroupListType"
              aria-haspopup="true"
              :class="{ active: opition.type_group_view != null }"
              aria-controls="overlay_menuGroupListTypeButs"
            >
              <a
                ><i
                  style="margin-right: 5px"
                  class="pi pi-bars"
                ></i
                >Nhóm dữ liệu<i
                  style="margin-left: 5px"
                  class="pi pi-angle-down"
                ></i
              ></a>
            </li>
            <li
              @click="toggleFilter"
              aria-haspopup="true"
              :class="{
                active:
                  opition.filter_type != 0 ||
                  opition.filter_duan ||
                  opition.filter_taskgroup,
              }"
              aria-controls="overlay_Export"
            >
              <a
                ><i
                  style="margin-right: 5px"
                  class="pi pi-filter"
                ></i
                >{{ opition.loctitle
                }}<i
                  style="margin-left: 5px"
                  class="pi pi-angle-down"
                ></i
              ></a>
            </li>
            <li
              @click="toggleSort"
              :class="{ active: opition.sort }"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            >
              <a
                ><i class="pi pi-sort"></i> Sắp xếp
                <i class="pi pi-angle-down"></i
              ></a>
            </li>
            <li @click="onRefresh">
              <a><i class="pi pi-refresh"></i> Tải lại</a>
            </li>
          </ul>
          <OverlayPanel
            ref="menuFilterButs"
            id="task_filter"
            style="z-index: 10"
          >
            <div
              style="
                min-height: calc(100vh - 250px);
                max-height: calc(100vh - 250px);
                width: 100%;
                overflow-x: scroll;
              "
            >
              <ul
                v-for="(item, index) in itemFilterButs"
                :key="index"
                style="padding: 0px; margin: 0px"
              >
                <li
                  v-if="item.istype == 5 || item.istype == 6"
                  :class="{
                    children: item.hasChildren,
                    parent: !item.hasChildren,
                  }"
                  class="p-menuitem"
                >
                  <ul style="padding: 0px; display: flex; flex-direction: row">
                    <li
                      style="
                        list-style: none;
                        /* padding: 10px; */
                        /* display: flex; */
                        flex: 1;
                        align-items: center;
                      "
                      v-for="(item1, index) in item.groups"
                      :key="index"
                    >
                      <div
                        v-if="item1.is_children == 1 || item1.is_children == 3"
                      >
                        <a
                          @click="ChangeFilter(item.istype, false)"
                          :class="{ active: item.active }"
                        >
                          <i
                            style="padding-right: 5px"
                            :class="item1.icon"
                          ></i>
                          {{ item1.label }}
                        </a>
                        <span style="margin-left: 10px">
                          <Calendar
                            @date-select="ChangeFilter(item.istype, false)"
                            inputId="icon"
                            :showTime="true"
                            v-model="item.filter_date"
                            :showIcon="true"
                            :manualInput="true"
                          />
                        </span>
                      </div>
                      <div
                        v-if="item1.is_children != 1 && item1.is_children != 3"
                        style="display: flex; align-items: center"
                      >
                        <a
                          style="flex: 1"
                          @click="ChangeFilter(item.istype, false)"
                          >{{ item1.label }}
                        </a>
                        <span style="margin-left: 10px; flex: auto">
                          <Dropdown
                            @change="ChangeFilterAdvanced(item1.is_children)"
                            v-if="item1.is_children == 2"
                            :filter="true"
                            v-model="opition.filter_duan"
                            panelClass="d-design-dropdown"
                            selectionLimit="1"
                            :options="listDropdownProject"
                            optionLabel="project_name"
                            optionValue="project_id"
                            spellcheck="false"
                            class="col-9 ip36 p-0"
                            placeholder="Chọn"
                          >
                            <template #option="slotProps">
                              <div class="country-item flex">
                                <div class="pt-1">
                                  {{ slotProps.option.project_name }}
                                </div>
                              </div>
                            </template>
                          </Dropdown>
                          <Dropdown
                            v-if="item1.is_children == 4"
                            @change="ChangeFilterAdvanced(item1.is_children)"
                            :filter="true"
                            v-model="opition.filter_taskgroup"
                            panelClass="d-design-dropdown"
                            selectionLimit="1"
                            :options="listDropdownTaskGroup"
                            optionLabel="group_name"
                            optionValue="group_id"
                            spellcheck="false"
                            class="col-9 ip36 p-0"
                            placeholder="Chọn"
                          >
                            <template #option="slotProps">
                              <div class="country-item flex">
                                <div class="pt-1">
                                  {{ slotProps.option.group_name }}
                                </div>
                              </div>
                            </template>
                          </Dropdown>
                        </span>
                      </div>
                    </li>
                  </ul>
                </li>
                <li
                  v-if="item.istype == 4"
                  :class="{
                    children: item.hasChildren,
                    parent: !item.hasChildren,
                  }"
                  class="p-menuitem"
                >
                  <a :class="{ active: item.active }"
                    ><i
                      style="padding-right: 5px"
                      :class="item.icon"
                    ></i
                    >{{ item.label }}</a
                  >
                  <ul style="padding: 0px; display: flex">
                    <li
                      style="
                        list-style: none;
                        padding: 10px;
                        font-weight: bold;
                        display: flex;
                        flex-direction: column;
                      "
                      v-for="(item1, index) in item.groups"
                      :key="index"
                    >
                      <div style="padding-bottom: 10px">
                        <span>{{ item1.label }}</span>
                        <span
                          style="
                            color: #2196f3;
                            font-weight: bold;
                            margin-left: 5px;
                            font-size: 14px;
                          "
                          v-if="item1.is_change == 1"
                          >{{ filterTime1 }}
                          <i
                            @click="removeTime(item1.is_change)"
                            v-if="filterTime1"
                            style="color: black"
                            class="pi pi-times-circle"
                          ></i
                        ></span>
                        <span
                          style="
                            color: #2196f3;
                            font-weight: bold;
                            margin-left: 5px;
                            font-size: 14px;
                          "
                          v-if="item1.is_change == 2"
                          >{{ filterTime2 }}
                          <i
                            @click="removeTime(item1.is_change)"
                            v-if="filterTime2"
                            style="color: black"
                            class="pi pi-times-circle"
                          ></i
                        ></span>
                      </div>
                      <Calendar
                        v-if="item1.is_change == 1"
                        @date-select="
                          ChangeTimeFilter(item1.is_change, opition.sdate)
                        "
                        v-model="opition.sdate"
                        :showTime="true"
                        id="filterTime1"
                        :inline="true"
                        :manualInput="true"
                      />
                      <Calendar
                        v-if="item1.is_change == 2"
                        @date-select="
                          ChangeTimeFilter(item1.is_change, opition.edate)
                        "
                        v-model="opition.edate"
                        :showTime="true"
                        id="filterTime2"
                        :inline="true"
                      />
                    </li>
                  </ul>
                </li>
                <li
                  v-if="
                    item.istype == 1 || item.istype == 2 || item.istype == 3
                  "
                  :class="{
                    children: item.hasChildren,
                    parent: !item.hasChildren,
                  }"
                  class="p-menuitem"
                  @click="ChangeFilter(item.istype, false)"
                >
                  <a :class="{ active: item.active }"
                    ><i
                      style="padding-right: 5px"
                      :class="item.icon"
                    ></i
                    >{{ item.label }}</a
                  >
                </li>
              </ul>
            </div>
            <div style="float: right; padding: 10px">
              <Button
                @click="ChangeFilter(opition.filter_type, true)"
                label="Thực hiện"
              />``
              <Button
                @click="Del_ChangeFilter"
                id="btn_huy"
                style="
                  background-color: #f2f4f6;
                  border: 1px solid #f2f4f6;
                  color: #333;
                  margin-left: 10px;
                "
                label="Hủy lọc"
              />
            </div>
          </OverlayPanel>
          <Menu
            id="task_list_type"
            :model="itemListTypeButs"
            ref="menuListTypeButs"
            :popup="true"
          >
            <template #item="{ item }">
              <div @click="ChangeView(item)">
                <a :class="{ active: item.active }"
                  ><i :class="item.icon"></i>{{ item.label }}</a
                >
              </div>
            </template>
          </Menu>
          <Menu
            id="task_group_list_type"
            :model="itemGroupListTypeButs"
            ref="menuGroupListTypeButs"
            :popup="true"
          >
            <template #item="{ item }">
              <div @click="ChangeGroupView(item.type)">
                <a :class="{ active: item.active }"
                  ><i :class="item.icon"></i>{{ item.label }}</a
                >
              </div>
            </template>
          </Menu>
          <Menu
            id="task_sort"
            :model="itemSortButs"
            ref="menuSortButs"
            :popup="true"
          >
            <template #item="{ item }">
              <a
                @click="ChangeSortTask(item.sort, item.ob)"
                :class="{ active: item.active }"
                >{{ item.label }}</a
              >
            </template>
          </Menu>
        </template>
      </Toolbar>
    </div>
    <div style="display: flex; justify-content: center; margin-bottom: 10px">
      <ul
        id="header_bottom"
        style="padding: 0px; margin: 0px; display: flex"
      >
        <li
          @click="ChangeData(-1)"
          class="header-bottom"
          :class="{ active: opition.IsType == -1 }"
        >
          <a><i class="pi pi-bars"></i> Tất cả ({{ opition.totalAlls }})</a>
        </li>
        <li
          @click="ChangeData(0)"
          class="header-bottom"
          :class="{ active: opition.IsType == 0 }"
        >
          <a
            ><i class="pi pi-user-edit"></i> Tôi làm ({{
              opition.total_toilam
            }})</a
          >
        </li>
        <li
          @click="ChangeData(1)"
          class="header-bottom"
          :class="{ active: opition.IsType == 1 }"
        >
          <a><i class="pi pi-user"></i> Quản lý ({{ opition.total_quanly }})</a>
        </li>
        <li
          @click="ChangeData(2)"
          class="header-bottom"
          :class="{ active: opition.IsType == 2 }"
        >
          <a
            ><i class="pi pi-users"></i> Theo dõi ({{
              opition.total_theodoi
            }})</a
          >
        </li>
        <li
          @click="ChangeData(3)"
          class="header-bottom"
          :class="{ active: opition.IsType == 3 }"
        >
          <a
            ><i class="pi pi-user-plus"></i> Tôi tạo ({{
              opition.total_toitao
            }})</a
          >
        </li>
        <li
          @click="ChangeData(4)"
          class="header-bottom"
          :class="{ active: opition.IsType == 4 }"
        >
          <a
            ><i class="pi pi-check-circle"></i> Hoàn thành ({{
              opition.total_hoanthanh
            }})</a
          >
        </li>
        <li
          @click="ChangeData(5)"
          class="header-bottom"
          :class="{ active: opition.IsType == 5 }"
        >
          <a
            ><i class="pi pi-calendar-times"></i> Quá hạn ({{
              opition.total_quahan
            }})</a
          >
        </li>
      </ul>
    </div>
    <!-- kiểu LIST -->
    <DataTable
      id="task"
      v-if="opition.type_view == 1"
      v-model:first="first"
      :rowHover="true"
      :value="listTask"
      :paginator="true"
      :rows="opition.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      :totalRecords="opition.totalRecords"
      :row-hover="true"
      dataKey="task_id"
      v-model:selection="selectedTasks"
      @page="onPage($event)"
      @sort="onSort($event)"
      @filter="onFilter($event)"
      :lazy="true"
      rowGroupMode="subheader"
      :groupRowsBy="
        opition.type_group_view == 1 && opition.type_group_view
          ? 'project_name'
          : opition.type_group_view == 2 && opition.type_group_view
          ? 'group_name'
          : ''
      "
      :expandableRowGroups="opition.type_group_view ? true : false"
      v-model:expandedRowGroups="expandedRowGroups"
      @rowgroupExpand="onRowGroupExpand($event)"
      @rowgroupCollapse="onRowGroupCollapse($event)"
      selectionMode="single"
      @rowSelect="onRowSelect($event.data)"
      @rowUnselect="onRowUnselect($event.data)"
    >
      <template
        v-if="opition.type_group_view != null"
        #groupheader="slotProps"
      >
        <span>{{
          opition.type_group_view == 1
            ? slotProps.data.project_name
            : slotProps.data.group_name
        }}</span>
      </template>
      <Column
        field="STT"
        headerStyle="text-align:center;max-width:4rem;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:4rem;"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        headerStyle="text-align:center;max-width:50px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:50px; "
        class="align-items-center justify-content-center text-center"
      >
        <template #body="value">
          <Avatar
            v-tooltip.bottom="{
              value:
                value.data.full_name +
                '<br/>' +
                (value.data.tenChucVu || '') +
                '<br/>' +
                (value.data.tenToChuc || ''),
              escape: true,
            }"
            v-bind:label="
              value.data.avatar
                ? ''
                : (value.data.last_name ?? '').substring(0, 1)
            "
            v-bind:image="basedomainURL + value.data.avatar"
            style="
              background-color: #2196f3;
              color: #ffffff;
              width: 2.5rem;
              height: 2.5rem;
              font-size: 15px !important;
            "
            :style="{
              background: bgColor[0] + '!important',
            }"
            class="cursor-pointer"
            size="xlarge"
            shape="circle"
          />
        </template>
      </Column>
      <Column
        header="Tên công việc"
        headerStyle="min-height:3.125rem"
        bodyStyle=" "
      >
        <template #body="data">
          <div style="display: flex; flex-direction: column; padding: 5px">
            <div style="line-height: 20px; display: flex">
              <span
                v-tooltip="'Ưu tiên'"
                v-if="data.data.is_prioritize"
                style="margin-right: 5px"
                ><i
                  style="color: orange"
                  class="pi pi-star-fill"
                ></i
              ></span>
              <span
                style="
                  font-weight: bold;
                  font-size: 14px;
                  overflow: hidden;
                  text-overflow: ellipsis;
                  width: 100%;
                  display: -webkit-box;
                  -webkit-line-clamp: 2;
                  -webkit-box-orient: vertical;
                "
                >{{ data.data.task_name }}</span
              >
            </div>
            <div
              style="
                font-size: 12px;
                margin-top: 5px;
                display: flex;
                align-items: center;
              "
            >
              <span
                v-if="data.data.start_date || data.data.end_date"
                style="color: #98a9bc"
                >{{
                  data.data.start_date
                    ? moment(new Date(data.data.start_date)).format(
                        "DD/MM/YYYY",
                      )
                    : null
                }}
                -
                {{
                  data.data.end_date
                    ? moment(new Date(data.data.end_date)).format("DD/MM/YYYY")
                    : null
                }}</span
              >
              <span
                v-if="data.data.isQL"
                style="
                  background-color: #337ab7;
                  color: #ffffff;
                  display: inline;
                  padding: 0.4em 0.6em;
                  font-size: 75%;
                  font-weight: 700;
                  line-height: 1;
                  color: #fff;
                  text-align: center;
                  white-space: nowrap;
                  vertical-align: baseline;
                  border-radius: 0.25em;
                  margin-left: 10px;
                "
                >Quản lý</span
              >
              <span
                v-if="data.data.isTT"
                style="
                  background-color: #5cb85c;
                  color: #ffffff;
                  display: inline;
                  padding: 0.4em 0.6em;
                  font-size: 75%;
                  font-weight: 700;
                  line-height: 1;
                  color: #fff;
                  text-align: center;
                  white-space: nowrap;
                  vertical-align: baseline;
                  border-radius: 0.25em;
                  margin-left: 5px;
                "
                >Thực hiện</span
              >
              <span
                v-if="data.data.isTD"
                style="
                  background-color: #5bc0de;
                  color: #ffffff;
                  display: inline;
                  padding: 0.4em 0.6em;
                  font-size: 75%;
                  font-weight: 700;
                  line-height: 1;
                  color: #fff;
                  text-align: center;
                  white-space: nowrap;
                  vertical-align: baseline;
                  border-radius: 0.25em;
                  margin-left: 5px;
                "
                >Theo dõi</span
              >
            </div>
            <div
              v-if="data.data.project_name"
              style="
                min-height: 25px;
                display: flex;
                align-items: center;
                margin-top: 10px;
              "
            >
              <i class="pi pi-tag"></i>
              <span
                class="duan"
                style="
                  font-size: 13px;
                  font-weight: 400;
                  margin-left: 5px;
                  color: #0078d4;
                "
                >{{ data.data.project_name }}</span
              >
            </div>
          </div>
        </template>
      </Column>
      <Column
        field=""
        header="Thành viên"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:150px;"
      >
        <template #body="data">
          <AvatarGroup>
            <div
              v-for="(value, index) in data.data.ThanhvienShows"
              :key="index"
            >
              <div>
                <Avatar
                  v-tooltip.bottom="{
                    value:
                      value.type_name +
                      ': ' +
                      value.fullName +
                      '<br/>' +
                      (value.tenChucVu || '') +
                      '<br/>' +
                      (value.tenToChuc || ''),
                    escape: true,
                  }"
                  v-bind:label="
                    value.avatar ? '' : (value.ten ?? '').substring(0, 1)
                  "
                  v-bind:image="basedomainURL + value.avatar"
                  style="
                    background-color: #2196f3;
                    color: #ffffff;
                    width: 32px;
                    height: 32px;
                    font-size: 15px !important;
                    margin-left: -10px;
                  "
                  :style="{
                    background: bgColor[index % 7] + '!important',
                  }"
                  class="cursor-pointer"
                  size="xlarge"
                  shape="circle"
                />
              </div>
            </div>
            <Avatar
              v-if="
                data.data.Thanhviens.length - data.data.ThanhvienShows.length >
                0
              "
              :label="
                '+' +
                (data.data.Thanhviens.length -
                  data.data.ThanhvienShows.length) +
                ''
              "
              class="cursor-pointer"
              shape="circle"
              style="
                background-color: #e9e9e9 !important;
                color: #98a9bc;
                font-size: 14px !important;
                width: 32px;
                margin-left: -10px;
                height: 32px;
              "
            />
          </AvatarGroup>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        header="Tiến độ"
        headerStyle="text-align:center;max-width:100px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:100px;"
      >
        <template #body="data">
          <span v-if="data.data.progress == 0">{{ data.data.progress }} %</span>
          <div
            v-if="data.data.progress != 0"
            style="width: 100%"
          >
            <ProgressBar :value="data.data.progress" />
          </div>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        header="Thời gian xử lý"
        headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:150px;"
      >
        <template #body="data">
          <div v-if="data.data.title_time">
            <span
              style="
                font-size: 10px;
                font-weight: bold;
                padding: 5px;
                border-radius: 5px;
              "
              :style="{
                background: data.data.time_bg,
                color: data.data.status_text_color,
              }"
              >{{ data.data.title_time }}</span
            >
          </div>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        header="Ngày kết thúc"
        headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:150px;"
      >
        <template #body="data">
          <div
            v-if="data.data.is_deadline == true"
            style="
              background-color: #fff8ee;
              padding: 10px 20px;
              border-radius: 5px;
            "
          >
            <span style="color: #ffab2b; font-size: 13px; font-weight: bold"
              >{{
                moment(new Date(data.data.end_date)).format("DD/MM/YYYY HH:mm")
              }}
            </span>
          </div>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        header="Trạng thái"
        headerStyle="text-align:center;max-width:120px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:120px;"
      >
        <template #body="data">
          <Chip
            :style="{
              background: data.data.status_bg_color,
              color: data.data.status_text_color,
            }"
            v-bind:label="data.data.status_name"
          />
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;min-height:3.125rem;max-width:100px;"
        bodyStyle="text-align:center;max-width:100px;"
      >
        <template #body="data">
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == data.data.created_by ||
              data.data.isEdit == true ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id == data.data.organization_id)
            "
          >
            <Button
              @click="editTask(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="DelTask(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              v-tooltip="'Xóa'"
            ></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          style="
            min-height: calc(100vh - 215px);
            max-height: calc(100vh - 215px);
            display: flex;
            flex-direction: column;
          "
          v-if="listTask != null"
        >
          <img
            src="../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
    <!-- end -->
    <!-- paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :paginator="true" 
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            :rowsPerPageOptions="[20, 30, 50, 100, 200]"
     -->
    <!-- kiểu TREE -->
    <div
      id="task-tree"
      style="overflow-x: auto"
      v-if="
        opition.type_view == 2 &&
        (opition.type_group_view == 1 || opition.type_group_view == 2)
      "
    >
      <div
        v-for="(l, index) in listTask"
        :key="index"
      >
        <div
          class="task-tree-lable"
          @click="ChangeShowListCVGroup(l)"
          style="
            height: 40px;
            display: flex;
            align-items: center;
            background-color: rgb(220 220 220);
            font-weight: bold;
            padding: 10px;
            border-bottom: 1px solid #aaa;
          "
        >
          <i
            style="margin-right: 5px; font-weight: bold"
            :class="
              l.isShow == false ? 'pi pi-angle-right' : 'pi pi-angle-down'
            "
          ></i>
          {{ l.group_view_name }} ({{ l.CVGroup.length }})
        </div>
        <TreeTable
          style="height: auto"
          v-if="l.CVGroup.length > 0 && l.isShow == true"
          sortMode="single"
          ref="dt"
          @nodeSelect="onNodeSelect"
          :value="l.CVGroup"
          :paginator="false"
          :rows="opition.PageSize"
          :scrollable="true"
          scrollHeight="flex"
          v-model:selectionKeys="selectedKeys"
          v-model:first="first"
          :loading="opition.loading"
          :expandedKeys="expandedKeys"
          :rowHover="true"
          responsiveLayout="scroll"
          :totalRecords="opition.totalRecords"
          selectionMode="single"
          filterMode="lenient"
          @page="onPage($event)"
        >
          <Column
            field="STT"
            headerStyle="text-align:center;max-width:75px;height:50px"
            bodyStyle="text-align:center;max-width:50px;;max-height:600px"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="menu">
              <div
                v-if="menu.node.data.parent_id == null"
                style="font-weight: 1000"
              >
                {{ menu.node.data.STT2 }}
              </div>
              <div
                v-else
                style="font-weight: 500"
              >
                {{ menu.node.data.STT2 }}
              </div>
            </template>
          </Column>
          <Column
            headerStyle="text-align:center;max-width:50px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:100px; "
            :expander="true"
            class="align-items-center justify-content-left text-center"
          >
            <template #body="value">
              <Avatar
                v-tooltip.bottom="{
                  value:
                    value.node.data.full_name +
                    '<br/>' +
                    (value.node.data.tenChucVu || '') +
                    '<br/>' +
                    (value.node.data.tenToChuc || ''),
                  escape: true,
                }"
                v-bind:label="
                  value.node.data.avatar
                    ? ''
                    : (value.node.data.last_name ?? '').substring(0, 1)
                "
                v-bind:image="basedomainURL + value.node.data.avatar"
                style="
                  background-color: #2196f3;
                  color: #ffffff;
                  width: 2.5rem;
                  height: 2.5rem;
                  font-size: 15px !important;
                "
                :style="{
                  background: bgColor[0] + '!important',
                }"
                class="cursor-pointer"
                size="xlarge"
                shape="circle"
              />
            </template>
          </Column>
          <Column
            header="Tên công việc"
            headerStyle="min-height:3.125rem"
            bodyStyle=" "
          >
            <template #body="data">
              <div style="display: flex; flex-direction: column; padding: 5px">
                <div style="line-height: 20px; display: flex">
                  <span
                    v-tooltip="'Ưu tiên'"
                    v-if="data.node.data.is_prioritize"
                    style="margin-right: 5px"
                    ><i
                      style="color: orange"
                      class="pi pi-star-fill"
                    ></i
                  ></span>
                  <span
                    style="
                      font-weight: bold;
                      font-size: 14px;
                      overflow: hidden;
                      text-overflow: ellipsis;
                      width: 100%;
                      display: -webkit-box;
                      -webkit-line-clamp: 2;
                      -webkit-box-orient: vertical;
                    "
                    >{{ data.node.data.task_name }}</span
                  >
                </div>
                <div style="font-size: 12px; margin-top: 5px">
                  <span
                    v-if="data.node.data.start_date || data.node.data.end_date"
                    style="color: #98a9bc"
                    >{{
                      data.node.data.start_date
                        ? moment(new Date(data.node.data.start_date)).format(
                            "DD/MM/YYYY",
                          )
                        : null
                    }}
                    -
                    {{
                      data.node.data.end_date
                        ? moment(new Date(data.node.data.end_date)).format(
                            "DD/MM/YYYY",
                          )
                        : null
                    }}</span
                  >
                  <span
                    v-if="data.node.data.isQL"
                    style="
                      background-color: #337ab7;
                      color: #ffffff;
                      display: inline;
                      padding: 0.4em 0.6em;
                      font-size: 75%;
                      font-weight: 700;
                      line-height: 1;
                      color: #fff;
                      text-align: center;
                      white-space: nowrap;
                      vertical-align: baseline;
                      border-radius: 0.25em;
                      margin-left: 10px;
                    "
                    >Quản lý</span
                  >
                  <span
                    v-if="data.node.data.isTT"
                    style="
                      background-color: #5cb85c;
                      color: #ffffff;
                      display: inline;
                      padding: 0.4em 0.6em;
                      font-size: 75%;
                      font-weight: 700;
                      line-height: 1;
                      color: #fff;
                      text-align: center;
                      white-space: nowrap;
                      vertical-align: baseline;
                      border-radius: 0.25em;
                      margin-left: 5px;
                    "
                    >Thực hiện</span
                  >
                  <span
                    v-if="data.node.data.isTD"
                    style="
                      background-color: #5bc0de;
                      color: #ffffff;
                      display: inline;
                      padding: 0.4em 0.6em;
                      font-size: 75%;
                      font-weight: 700;
                      line-height: 1;
                      color: #fff;
                      text-align: center;
                      white-space: nowrap;
                      vertical-align: baseline;
                      border-radius: 0.25em;
                      margin-left: 5px;
                    "
                    >Theo dõi</span
                  >
                </div>
                <div
                  v-if="data.node.data.project_name"
                  style="
                    min-height: 25px;
                    display: flex;
                    align-items: center;
                    margin-top: 10px;
                  "
                >
                  <i class="pi pi-tag"></i>
                  <span
                    class="duan"
                    style="
                      font-size: 13px;
                      font-weight: 400;
                      margin-left: 5px;
                      color: #0078d4;
                    "
                    >{{ data.node.data.project_name }}</span
                  >
                </div>
              </div>
            </template>
          </Column>
          <Column
            field=""
            header="Thành viên"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:150px;"
          >
            <template #body="data">
              <AvatarGroup>
                <div
                  v-for="(value, index) in data.node.data.ThanhvienShows"
                  :key="index"
                >
                  <div>
                    <Avatar
                      v-tooltip.bottom="{
                        value:
                          value.type_name +
                          ': ' +
                          value.fullName +
                          '<br/>' +
                          (value.tenChucVu || '') +
                          '<br/>' +
                          (value.tenToChuc || ''),
                        escape: true,
                      }"
                      v-bind:label="
                        value.avatar ? '' : (value.ten ?? '').substring(0, 1)
                      "
                      v-bind:image="basedomainURL + value.avatar"
                      style="
                        background-color: #2196f3;
                        color: #ffffff;
                        width: 32px;
                        height: 32px;
                        font-size: 15px !important;
                        margin-left: -10px;
                      "
                      :style="{
                        background: bgColor[index % 7] + '!important',
                      }"
                      class="cursor-pointer"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                </div>
                <Avatar
                  v-if="
                    data.node.data.Thanhviens.length -
                      data.node.data.ThanhvienShows.length >
                    0
                  "
                  :label="
                    '+' +
                    (data.node.data.Thanhviens.length -
                      data.node.data.ThanhvienShows.length) +
                    ''
                  "
                  class="cursor-pointer"
                  shape="circle"
                  style="
                    background-color: #e9e9e9 !important;
                    color: #98a9bc;
                    font-size: 14px !important;
                    width: 32px;
                    margin-left: -10px;
                    height: 32px;
                  "
                />
              </AvatarGroup>
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            header="Tiến độ"
            headerStyle="text-align:center;max-width:100px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:100px;"
          >
            <template #body="data">
              <span v-if="data.node.data.progress == 0"
                >{{ data.node.data.progress }} %</span
              >
              <div
                v-if="data.node.data.progress != 0"
                style="width: 100%"
              >
                <ProgressBar :value="data.node.data.progress" />
              </div>
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            header="Thời gian xử lý"
            headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:150px;"
          >
            <template #body="data">
              <div v-if="data.node.data.title_time">
                <span
                  style="
                    font-size: 10px;
                    font-weight: bold;
                    padding: 5px;
                    border-radius: 5px;
                  "
                  :style="{
                    background: data.node.data.time_bg,
                    color: data.node.data.status_text_color,
                  }"
                  >{{ data.node.data.title_time }}</span
                >
              </div>
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            header="Ngày kết thúc"
            headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:150px;"
          >
            <template #body="data">
              <div
                v-if="data.node.data.is_deadline == true"
                style="
                  background-color: #fff8ee;
                  padding: 10px 20px;
                  border-radius: 5px;
                "
              >
                <span
                  style="color: #ffab2b; font-size: 13px; font-weight: bold"
                  >{{
                    moment(new Date(data.node.data.end_date)).format(
                      "DD/MM/YYYY HH:mm",
                    )
                  }}</span
                >
              </div>
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            header="Trạng thái"
            headerStyle="text-align:center;max-width:120px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:120px;"
          >
            <template #body="data">
              <Chip
                :style="{
                  background: data.node.data.status_bg_color,
                  color: data.node.data.status_text_color,
                }"
                v-bind:label="data.node.data.status_name"
              />
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;min-height:3.125rem;max-width:100px;"
            bodyStyle="text-align:center;max-width:100px;"
          >
            <template #body="data">
              <div
                v-if="
                  store.state.user.is_super == true ||
                  store.state.user.user_id == data.node.data.created_by ||
                  data.node.data.isEdit == true ||
                  (store.state.user.role_id == 'admin' &&
                    store.state.user.organization_id ==
                      data.node.data.organization_id)
                "
              >
                <Button
                  @click="editTask(data.node.data)"
                  class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                  type="button"
                  icon="pi pi-pencil"
                  v-tooltip="'Sửa'"
                ></Button>
                <Button
                  @click="DelTask(data.node.data)"
                  class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                  type="button"
                  icon="pi pi-trash"
                  v-tooltip="'Xóa'"
                ></Button>
              </div>
            </template>
          </Column>

          <template #empty>
            <div
              class="align-items-center justify-content-center p-4 text-center m-auto"
              style="
                min-height: calc(100vh - 215px);
                max-height: calc(100vh - 215px);
                display: flex;
                flex-direction: column;
              "
              v-if="listTask != null || opition.totalRecords == 0"
            >
              <img
                src="../../assets/background/nodata.png"
                height="144"
              />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </template>
        </TreeTable>
      </div>
    </div>
    <TreeTable
      id="task-tree"
      v-if="opition.type_view == 2 && opition.type_group_view == null"
      sortMode="single"
      ref="dt"
      @nodeSelect="onNodeSelect"
      :value="listTask"
      :paginator="false"
      :rows="opition.PageSize"
      :scrollable="true"
      scrollHeight="flex"
      v-model:selectionKeys="selectedKeys"
      v-model:first="first"
      :loading="opition.loading"
      :expandedKeys="expandedKeys"
      :rowHover="true"
      responsiveLayout="scroll"
      :totalRecords="opition.totalRecords"
      selectionMode="single"
      filterMode="lenient"
      @page="onPage($event)"
    >
      <Column
        field="STT"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:50px;;max-height:600px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="menu">
          <div
            v-if="menu.node.data.parent_id == null"
            style="font-weight: 1000"
          >
            {{ menu.node.data.STT2 }}
          </div>
          <div
            v-else
            style="font-weight: 500"
          >
            {{ menu.node.data.STT2 }}
          </div>
        </template>
      </Column>
      <Column
        headerStyle="text-align:center;max-width:50px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:100px; "
        :expander="true"
        class="align-items-center justify-content-left text-center"
      >
        <template #body="value">
          <Avatar
            v-tooltip.bottom="{
              value:
                value.node.data.full_name +
                '<br/>' +
                (value.node.data.tenChucVu || '') +
                '<br/>' +
                (value.node.data.tenToChuc || ''),
              escape: true,
            }"
            v-bind:label="
              value.node.data.avatar
                ? ''
                : (value.node.data.last_name ?? '').substring(0, 1)
            "
            v-bind:image="basedomainURL + value.node.data.avatar"
            style="
              background-color: #2196f3;
              color: #ffffff;
              width: 2.5rem;
              height: 2.5rem;
              font-size: 15px !important;
            "
            :style="{
              background: bgColor[0] + '!important',
            }"
            class="cursor-pointer"
            size="xlarge"
            shape="circle"
          />
        </template>
      </Column>
      <Column
        header="Tên công việc"
        headerStyle="min-height:3.125rem"
        bodyStyle=" "
      >
        <template #body="data">
          <div style="display: flex; flex-direction: column; padding: 5px">
            <div style="line-height: 20px; display: flex">
              <span
                v-tooltip="'Ưu tiên'"
                v-if="data.node.data.is_prioritize"
                style="margin-right: 5px"
                ><i
                  style="color: orange"
                  class="pi pi-star-fill"
                ></i
              ></span>
              <span
                style="
                  font-weight: bold;
                  font-size: 14px;
                  overflow: hidden;
                  text-overflow: ellipsis;
                  width: 100%;
                  display: -webkit-box;
                  -webkit-line-clamp: 2;
                  -webkit-box-orient: vertical;
                "
                >{{ data.node.data.task_name }}</span
              >
            </div>
            <div style="font-size: 12px; margin-top: 5px">
              <span
                v-if="data.node.data.start_date || data.node.data.end_date"
                style="color: #98a9bc"
                >{{
                  data.node.data.start_date
                    ? moment(new Date(data.node.data.start_date)).format(
                        "DD/MM/YYYY",
                      )
                    : null
                }}
                -
                {{
                  data.node.data.end_date
                    ? moment(new Date(data.node.data.end_date)).format(
                        "DD/MM/YYYY",
                      )
                    : null
                }}</span
              >
              <span
                v-if="data.node.data.isQL"
                style="
                  background-color: #337ab7;
                  color: #ffffff;
                  display: inline;
                  padding: 0.4em 0.6em;
                  font-size: 75%;
                  font-weight: 700;
                  line-height: 1;
                  color: #fff;
                  text-align: center;
                  white-space: nowrap;
                  vertical-align: baseline;
                  border-radius: 0.25em;
                  margin-left: 10px;
                "
                >Quản lý</span
              >
              <span
                v-if="data.node.data.isTT"
                style="
                  background-color: #5cb85c;
                  color: #ffffff;
                  display: inline;
                  padding: 0.4em 0.6em;
                  font-size: 75%;
                  font-weight: 700;
                  line-height: 1;
                  color: #fff;
                  text-align: center;
                  white-space: nowrap;
                  vertical-align: baseline;
                  border-radius: 0.25em;
                  margin-left: 5px;
                "
                >Thực hiện</span
              >
              <span
                v-if="data.node.data.isTD"
                style="
                  background-color: #5bc0de;
                  color: #ffffff;
                  display: inline;
                  padding: 0.4em 0.6em;
                  font-size: 75%;
                  font-weight: 700;
                  line-height: 1;
                  color: #fff;
                  text-align: center;
                  white-space: nowrap;
                  vertical-align: baseline;
                  border-radius: 0.25em;
                  margin-left: 5px;
                "
                >Theo dõi</span
              >
            </div>
            <div
              v-if="data.node.data.project_name"
              style="
                min-height: 25px;
                display: flex;
                align-items: center;
                margin-top: 10px;
              "
            >
              <i class="pi pi-tag"></i>
              <span
                class="duan"
                style="
                  font-size: 13px;
                  font-weight: 400;
                  margin-left: 5px;
                  color: #0078d4;
                "
                >{{ data.node.data.project_name }}</span
              >
            </div>
          </div>
        </template>
      </Column>
      <Column
        field=""
        header="Thành viên"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:150px;"
      >
        <template #body="data">
          <AvatarGroup>
            <div
              v-for="(value, index) in data.node.data.ThanhvienShows"
              :key="index"
            >
              <div>
                <Avatar
                  v-tooltip.bottom="{
                    value:
                      value.type_name +
                      ': ' +
                      value.fullName +
                      '<br/>' +
                      (value.tenChucVu || '') +
                      '<br/>' +
                      (value.tenToChuc || ''),
                    escape: true,
                  }"
                  v-bind:label="
                    value.avatar ? '' : (value.ten ?? '').substring(0, 1)
                  "
                  v-bind:image="basedomainURL + value.avatar"
                  style="
                    background-color: #2196f3;
                    color: #ffffff;
                    width: 32px;
                    height: 32px;
                    font-size: 15px !important;
                    margin-left: -10px;
                  "
                  :style="{
                    background: bgColor[index % 7] + '!important',
                  }"
                  class="cursor-pointer"
                  size="xlarge"
                  shape="circle"
                />
              </div>
            </div>
            <Avatar
              v-if="
                data.node.data.Thanhviens.length -
                  data.node.data.ThanhvienShows.length >
                0
              "
              :label="
                '+' +
                (data.node.data.Thanhviens.length -
                  data.node.data.ThanhvienShows.length) +
                ''
              "
              class="cursor-pointer"
              shape="circle"
              style="
                background-color: #e9e9e9 !important;
                color: #98a9bc;
                font-size: 14px !important;
                width: 32px;
                margin-left: -10px;
                height: 32px;
              "
            />
          </AvatarGroup>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        header="Tiến độ"
        headerStyle="text-align:center;max-width:100px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:100px;"
      >
        <template #body="data">
          <span v-if="data.node.data.progress == 0"
            >{{ data.node.data.progress }} %</span
          >
          <div
            v-if="data.node.data.progress != 0"
            style="width: 100%"
          >
            <ProgressBar :value="data.node.data.progress" />
          </div>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        header="Thời gian xử lý"
        headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:150px;"
      >
        <template #body="data">
          <div v-if="data.node.data.title_time">
            <span
              style="
                font-size: 10px;
                font-weight: bold;
                padding: 5px;
                border-radius: 5px;
              "
              :style="{
                background: data.node.data.time_bg,
                color: data.node.data.status_text_color,
              }"
              >{{ data.node.data.title_time }}</span
            >
          </div>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        header="Ngày kết thúc"
        headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:150px;"
      >
        <template #body="data">
          <div
            v-if="data.node.data.is_deadline == true"
            style="
              background-color: #fff8ee;
              padding: 10px 20px;
              border-radius: 5px;
            "
          >
            <span style="color: #ffab2b; font-size: 13px; font-weight: bold">{{
              moment(new Date(data.node.data.end_date)).format(
                "DD/MM/YYYY HH:mm",
              )
            }}</span>
          </div>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        header="Trạng thái"
        headerStyle="text-align:center;max-width:120px;min-height:3.125rem"
        bodyStyle="text-align:center;max-width:120px;"
      >
        <template #body="data">
          <Chip
            :style="{
              background: data.node.data.status_bg_color,
              color: data.node.data.status_text_color,
            }"
            v-bind:label="data.node.data.status_name"
          />
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;min-height:3.125rem;max-width:100px;"
        bodyStyle="text-align:center;max-width:100px;"
      >
        <template #body="data">
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == data.node.data.created_by ||
              data.node.data.isEdit == true ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id ==
                  data.node.data.organization_id)
            "
          >
            <Button
              @click="editTask(data.node.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="DelTask(data.node.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              v-tooltip="'Xóa'"
            ></Button>
          </div>
        </template>
      </Column>

      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          style="
            min-height: calc(100vh - 215px);
            max-height: calc(100vh - 215px);
            display: flex;
            flex-direction: column;
          "
          v-if="listTask != null || opition.totalRecords == 0"
        >
          <img
            src="../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </TreeTable>
    <!-- end -->
    <!-- kiểu GRID -->
    <div
      id="task-grid"
      v-if="opition.type_view == 3"
      style="
        height: 85%;
        width: 100%;
        display: -webkit-box;
        overflow-x: auto;
        overflow-y: hidden;
      "
    >
      <div
        v-if="listTask.length > 0"
        class="md:col-md-3"
        v-for="(item, index) in listTask"
        :key="index"
        style="width: 320px; height: 100%; margin: 0px 10px"
      >
        <span
          style="
            padding: 10px;
            display: flex;
            justify-content: center;
            align-items: center;
            font-weight: bold;
            color: #ffffff;
          "
          :style="{
            background: item.group_view_bg_color + '!important',
          }"
          >{{ item.group_view_name }} ({{ item.CVGroup.length }})</span
        >
        <div
          style="width: 106%; height: 95%; overflow: hidden auto"
          id="task-grid"
          class="scroll-outer"
        >
          <div
            class="scroll-inner"
            style="width: fit-content"
          >
            <div
              id="type_group_view"
              v-if="opition.type_group_view != null"
            >
              <div
                v-for="(l, index) in item.ListCVGroup"
                :key="index"
              >
                <span
                  @click="ChangeShowListCVGroup(l)"
                  style="
                    padding: 10px;
                    display: flex;
                    align-items: center;
                    font-weight: bold;
                    color: #ffffff;
                    width: 320px;
                  "
                  :style="{
                    background: l.group_view_bg_color + '!important',
                  }"
                  ><i
                    style="margin-right: 5px"
                    :class="
                      l.isShow == false
                        ? 'pi pi-angle-right'
                        : 'pi pi-angle-down'
                    "
                  ></i
                  >{{ l.group_view_name }} ({{ l.CVGroup2.length }})</span
                >
                <div v-if="l.isShow">
                  <Card
                    v-if="l.CVGroup2.length > 0"
                    v-for="(cv, index) in l.CVGroup2"
                    :key="index"
                    style="width: 320px; border-bottom: 1px solid #ccc"
                  >
                    <template #title>
                      <span
                        @click="onRowSelect(cv)"
                        v-tooltip="'Xem chi tiết'"
                        style="
                          overflow: hidden;
                          font-size: 14px;
                          font-weight: bold;
                          text-overflow: ellipsis;
                          width: 100%;
                          display: -webkit-box;
                          -webkit-line-clamp: 2;
                          -webkit-box-orient: vertical;
                        "
                      >
                        {{ cv.task_name }}
                      </span>
                    </template>
                    <template #content>
                      <span
                        v-if="cv.project_name"
                        style="
                          margin: 0px auto;
                          text-align: center;
                          padding: 5px 15px;
                          background-color: #f2f4f6;
                          max-width: max-content;
                          border-radius: 5px;
                          overflow: hidden;
                          text-overflow: ellipsis;
                          white-space: nowrap;
                          max-width: 100%;
                          font-weight: 500;
                        "
                        >{{ cv.project_name }}</span
                      >
                      <span
                        v-if="cv.start_date || cv.end_date"
                        style="color: #98a9bc"
                        ><i
                          style="margin-right: 5px"
                          class="pi pi-calendar"
                        ></i
                        >{{
                          cv.start_date
                            ? moment(new Date(cv.start_date)).format(
                                "DD/MM/YYYY",
                              )
                            : null
                        }}
                        -
                        {{
                          cv.end_date
                            ? moment(new Date(cv.end_date)).format("DD/MM/YYYY")
                            : null
                        }}</span
                      >
                      <span>
                        <span
                          v-if="cv.isQL"
                          style="
                            background-color: #337ab7;
                            color: #ffffff;
                            display: inline;
                            padding: 0.4em 0.6em;
                            font-size: 75%;
                            font-weight: 700;
                            line-height: 1;
                            color: #fff;
                            text-align: center;
                            white-space: nowrap;
                            vertical-align: baseline;
                            border-radius: 0.25em;
                            margin-left: 10px;
                          "
                          >Quản lý</span
                        >
                        <span
                          v-if="cv.isTT"
                          style="
                            background-color: #5cb85c;
                            color: #ffffff;
                            display: inline;
                            padding: 0.4em 0.6em;
                            font-size: 75%;
                            font-weight: 700;
                            line-height: 1;
                            color: #fff;
                            text-align: center;
                            white-space: nowrap;
                            vertical-align: baseline;
                            border-radius: 0.25em;
                            margin-left: 5px;
                          "
                          >Thực hiện</span
                        >
                        <span
                          v-if="cv.isTD"
                          style="
                            background-color: #5bc0de;
                            color: #ffffff;
                            display: inline;
                            padding: 0.4em 0.6em;
                            font-size: 75%;
                            font-weight: 700;
                            line-height: 1;
                            color: #fff;
                            text-align: center;
                            white-space: nowrap;
                            vertical-align: baseline;
                            border-radius: 0.25em;
                            margin-left: 5px;
                          "
                          >Theo dõi</span
                        >
                      </span>
                      <span
                        style="
                          display: flex;
                          justify-content: center;
                          align-items: center;
                        "
                      >
                        <AvatarGroup>
                          <div
                            v-for="(value, index) in cv.ThanhvienShows"
                            :key="index"
                          >
                            <div>
                              <Avatar
                                v-tooltip.bottom="{
                                  value:
                                    value.type_name +
                                    ': ' +
                                    value.fullName +
                                    '<br/>' +
                                    (value.tenChucVu || '') +
                                    '<br/>' +
                                    (value.tenToChuc || ''),
                                  escape: true,
                                }"
                                v-bind:label="
                                  value.avatar
                                    ? ''
                                    : (value.ten ?? '').substring(0, 1)
                                "
                                v-bind:image="basedomainURL + value.avatar"
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  width: 32px;
                                  height: 32px;
                                  font-size: 15px !important;
                                  margin-left: -10px;
                                "
                                :style="{
                                  background: bgColor[index % 7] + '!important',
                                }"
                                class="cursor-pointer"
                                size="xlarge"
                                shape="circle"
                              />
                            </div>
                          </div>
                          <Avatar
                            v-if="
                              cv.Thanhviens.length - cv.ThanhvienShows.length >
                              0
                            "
                            :label="
                              '+' +
                              (cv.Thanhviens.length -
                                cv.ThanhvienShows.length) +
                              ''
                            "
                            class="cursor-pointer"
                            shape="circle"
                            style="
                              background-color: #e9e9e9 !important;
                              color: #98a9bc;
                              font-size: 14px !important;
                              width: 32px;
                              margin-left: -10px;
                              height: 32px;
                            "
                          />
                        </AvatarGroup>
                      </span>
                      <span
                        v-if="cv.title_time"
                        style="
                          width: max-content;
                          font-size: 10px;
                          font-weight: bold;
                          padding: 5px;
                          border-radius: 5px;
                        "
                        :style="{
                          background: cv.time_bg,
                          color: cv.status_text_color,
                        }"
                        >{{ cv.title_time }}</span
                      >
                      <div
                        class="card-chucnang"
                        style="
                          display: none;
                          flex-direction: column;
                          position: absolute;
                          right: 10px;
                        "
                        v-if="
                          store.state.user.is_super == true ||
                          store.state.user.user_id == cv.created_by ||
                          cv.isEdit == true ||
                          (store.state.user.role_id == 'admin' &&
                            store.state.user.organization_id ==
                              cv.organization_id)
                        "
                      >
                        <Button
                          @click="editTask(cv)"
                          style="margin-bottom: 5px"
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                          type="button"
                          icon="pi pi-pencil"
                          v-tooltip="'Sửa'"
                        ></Button>
                        <Button
                          @click="DelTask(cv)"
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                          type="button"
                          icon="pi pi-trash"
                          v-tooltip="'Xóa'"
                        ></Button>
                      </div>
                    </template>
                    <template #footer>
                      <!-- <span v-if="cv.progress == 0">{{ cv.progress }} %</span> -->
                      <div
                        v-if="cv.progress != 0"
                        style="width: 100%"
                      >
                        <ProgressBar :value="cv.progress" />
                      </div>
                    </template>
                  </Card>
                </div>
              </div>
            </div>
            <Card
              v-if="opition.type_group_view == null"
              v-for="(cv, index) in item.CVGroup"
              :key="index"
              style="width: 320px; margin-bottom: 2em"
            >
              <template #title>
                <span
                  @click="onRowSelect(cv)"
                  v-tooltip="'Xem chi tiết'"
                  style="
                    overflow: hidden;
                    font-size: 14px;
                    font-weight: bold;
                    text-overflow: ellipsis;
                    width: 100%;
                    display: -webkit-box;
                    -webkit-line-clamp: 2;
                    -webkit-box-orient: vertical;
                  "
                >
                  {{ cv.task_name }}
                </span>
              </template>
              <template #content>
                <span
                  v-if="cv.project_name"
                  style="
                    margin: 0px auto;
                    text-align: center;
                    padding: 5px 15px;
                    background-color: #f2f4f6;
                    max-width: max-content;
                    border-radius: 5px;
                    overflow: hidden;
                    text-overflow: ellipsis;
                    white-space: nowrap;
                    max-width: 100%;
                    font-weight: 500;
                  "
                  >{{ cv.project_name }}</span
                >
                <span
                  v-if="cv.start_date || cv.end_date"
                  style="color: #98a9bc"
                  ><i
                    style="margin-right: 5px"
                    class="pi pi-calendar"
                  ></i
                  >{{
                    cv.start_date
                      ? moment(new Date(cv.start_date)).format("DD/MM/YYYY")
                      : null
                  }}
                  -
                  {{
                    cv.end_date
                      ? moment(new Date(cv.end_date)).format("DD/MM/YYYY")
                      : null
                  }}</span
                >
                <span>
                  <span
                    v-if="cv.isQL"
                    style="
                      background-color: #337ab7;
                      color: #ffffff;
                      display: inline;
                      padding: 0.4em 0.6em;
                      font-size: 75%;
                      font-weight: 700;
                      line-height: 1;
                      color: #fff;
                      text-align: center;
                      white-space: nowrap;
                      vertical-align: baseline;
                      border-radius: 0.25em;
                      margin-left: 10px;
                    "
                    >Quản lý</span
                  >
                  <span
                    v-if="cv.isTT"
                    style="
                      background-color: #5cb85c;
                      color: #ffffff;
                      display: inline;
                      padding: 0.4em 0.6em;
                      font-size: 75%;
                      font-weight: 700;
                      line-height: 1;
                      color: #fff;
                      text-align: center;
                      white-space: nowrap;
                      vertical-align: baseline;
                      border-radius: 0.25em;
                      margin-left: 5px;
                    "
                    >Thực hiện</span
                  >
                  <span
                    v-if="cv.isTD"
                    style="
                      background-color: #5bc0de;
                      color: #ffffff;
                      display: inline;
                      padding: 0.4em 0.6em;
                      font-size: 75%;
                      font-weight: 700;
                      line-height: 1;
                      color: #fff;
                      text-align: center;
                      white-space: nowrap;
                      vertical-align: baseline;
                      border-radius: 0.25em;
                      margin-left: 5px;
                    "
                    >Theo dõi</span
                  >
                </span>
                <span
                  style="
                    display: flex;
                    justify-content: center;
                    align-items: center;
                  "
                >
                  <AvatarGroup>
                    <div
                      v-for="(value, index) in cv.ThanhvienShows"
                      :key="index"
                    >
                      <div>
                        <Avatar
                          v-tooltip.bottom="{
                            value:
                              value.type_name +
                              ': ' +
                              value.fullName +
                              '<br/>' +
                              (value.tenChucVu || '') +
                              '<br/>' +
                              (value.tenToChuc || ''),
                            escape: true,
                          }"
                          v-bind:label="
                            value.avatar
                              ? ''
                              : (value.ten ?? '').substring(0, 1)
                          "
                          v-bind:image="basedomainURL + value.avatar"
                          style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 32px;
                            height: 32px;
                            font-size: 15px !important;
                            margin-left: -10px;
                          "
                          :style="{
                            background: bgColor[index % 7] + '!important',
                          }"
                          class="cursor-pointer"
                          size="xlarge"
                          shape="circle"
                        />
                      </div>
                    </div>
                    <Avatar
                      v-if="cv.Thanhviens.length - cv.ThanhvienShows.length > 0"
                      :label="
                        '+' +
                        (cv.Thanhviens.length - cv.ThanhvienShows.length) +
                        ''
                      "
                      class="cursor-pointer"
                      shape="circle"
                      style="
                        background-color: #e9e9e9 !important;
                        color: #98a9bc;
                        font-size: 14px !important;
                        width: 32px;
                        margin-left: -10px;
                        height: 32px;
                      "
                    />
                  </AvatarGroup>
                </span>
                <span
                  v-if="cv.title_time"
                  style="
                    width: max-content;
                    font-size: 10px;
                    font-weight: bold;
                    padding: 5px;
                    border-radius: 5px;
                  "
                  :style="{
                    background: cv.time_bg,
                    color: cv.status_text_color,
                  }"
                  >{{ cv.title_time }}</span
                >
                <div
                  class="card-chucnang"
                  style="
                    display: none;
                    flex-direction: column;
                    position: absolute;
                    right: 10px;
                  "
                  v-if="
                    store.state.user.is_super == true ||
                    store.state.user.user_id == cv.created_by ||
                    cv.isEdit == true ||
                    (store.state.user.role_id == 'admin' &&
                      store.state.user.organization_id == cv.organization_id)
                  "
                >
                  <Button
                    @click="editTask(cv)"
                    style="margin-bottom: 5px"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                    type="button"
                    icon="pi pi-pencil"
                    v-tooltip="'Sửa'"
                  ></Button>
                  <Button
                    @click="DelTask(cv)"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                    type="button"
                    icon="pi pi-trash"
                    v-tooltip="'Xóa'"
                  ></Button>
                </div>
              </template>
              <template #footer>
                <!-- <span v-if="cv.progress == 0">{{ cv.progress }} %</span> -->
                <div
                  v-if="cv.progress != 0"
                  style="width: 100%"
                >
                  <ProgressBar :value="cv.progress" />
                </div>
              </template>
            </Card>
          </div>
        </div>
      </div>
      <div
        class="align-items-center justify-content-center p-4 text-center m-auto"
        style="
          min-height: calc(100vh - 215px);
          max-height: calc(100vh - 215px);
          display: flex;
          flex-direction: column;
        "
        v-if="listTask.length == 0"
      >
        <img
          src="../../assets/background/nodata.png"
          height="144"
        />
        <h3 class="m-1">Không có dữ liệu</h3>
      </div>
    </div>
    <!-- end -->
    <!-- kiểu GANTT -->
    <div
      id="task-gantt"
      v-if="opition.type_view == 4"
      style="
        max-height: calc(100vh - 500px);
        min-height: calc(100vh - 150px);
        display: -webkit-box;
        overflow-x: auto;
        overflow-y: hidden;
      "
      class="grid formgrid m-2"
    >
      <div
        class="field col-12 md:col-12"
        style="
          display: flex;
          padding: 0px;
          max-height: calc(100vh - 500px);
          min-height: calc(100vh - 150px);
        "
      >
        <div
          class="col-12 scrollbox_delayed"
          style="height: 100%; padding: 0px; overflow: auto"
        >
          <table
            class="table table-border"
            style="
              width: max-content;
              table-layout: fixed;
              min-width: 100%;
              border-collapse: collapse;
              overflow-x: scroll;
            "
          >
            <thead
              style="
                background-color: #f8f9fa;
                position: sticky;
                top: 0px;
                z-index: 5;
              "
            >
              <tr>
                <th
                  class="fixcol left-0 p-3"
                  rowspan="3"
                  style="width: 200px; border: 1px solid #e9e9e9"
                >
                  Công việc
                </th>
                <th
                  class="fixcol left-200 p-3"
                  rowspan="3"
                  style="width: 150px; border: 1px solid #e9e9e9"
                >
                  Thực hiện
                </th>
                <th
                  class="fixcol left-350 p-3"
                  rowspan="3"
                  style="width: 100px; border: 1px solid #e9e9e9"
                >
                  Bắt đầu
                </th>
                <th
                  class="fixcol left-450 p-3"
                  rowspan="3"
                  style="width: 100px; border: 1px solid #e9e9e9"
                >
                  Kết thúc
                </th>
                <th
                  v-for="m in Grands"
                  class="p-3"
                  align="center"
                  :width="m.Dates.length * 40"
                  :colspan="m.Dates.length"
                  style="text-align: center; min-width: 100px; color: #2196f3"
                >
                  Tháng {{ m.Month }}/{{ m.Year }}
                </th>
                <!-- <th class="p-3" style="border: 1px solid #e9e9e9;" :colspan="GrandsDate.length">Tháng 2</th> -->
              </tr>
              <tr>
                <th
                  class="no-fixcol p-3"
                  width="40"
                  style="border: 1px solid #e9e9e9"
                  :style="
                    (g.bg == ''
                      ? 'background-color: #fff;'
                      : 'background-color:' + g.bg + ';',
                    'color:' + g.color)
                  "
                  v-for="g in GrandsDate"
                >
                  {{ g.DayName }}
                </th>
              </tr>
              <tr>
                <th
                  class="no-fixcol p-3"
                  width="40"
                  style="border: 1px solid #e9e9e9"
                  :style="
                    (g.bg == ''
                      ? 'background-color: #fff;'
                      : 'background-color:' + g.bg + ';',
                    'color:' + g.color)
                  "
                  v-for="g in GrandsDate"
                >
                  {{ g.DayN }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="l in listTask"
                @click="onRowSelect(l)"
              >
                <td
                  class="fixcol left-0 p-3"
                  style="border: 1px solid #e9e9e9; background-color: #f8f9fa"
                >
                  {{ l.task_name }}
                </td>
                <td
                  class="fixcol left-200 p-3"
                  style="border: 1px solid #e9e9e9; background-color: #f8f9fa"
                >
                  <div style="display: flex; justify-content: center">
                    <AvatarGroup>
                      <div
                        v-for="(value, index) in l.ThanhvienShows"
                        :key="index"
                      >
                        <div>
                          <Avatar
                            v-tooltip.bottom="{
                              value:
                                value.type_name +
                                ': ' +
                                value.fullName +
                                '<br/>' +
                                (value.tenChucVu || '') +
                                '<br/>' +
                                (value.tenToChuc || ''),
                              escape: true,
                            }"
                            v-bind:label="
                              value.avatar
                                ? ''
                                : (value.ten ?? '').substring(0, 1)
                            "
                            v-bind:image="basedomainURL + value.avatar"
                            style="
                              background-color: #2196f3;
                              color: #ffffff;
                              width: 32px;
                              height: 32px;
                              font-size: 15px !important;
                              margin-left: -10px;
                            "
                            :style="{
                              background: bgColor[index % 7] + '!important',
                            }"
                            class="cursor-pointer"
                            size="xlarge"
                            shape="circle"
                          />
                        </div>
                      </div>
                      <Avatar
                        v-if="l.Thanhviens.length - l.ThanhvienShows.length > 0"
                        :label="
                          '+' +
                          (l.Thanhviens.length - l.ThanhvienShows.length) +
                          ''
                        "
                        class="cursor-pointer"
                        shape="circle"
                        style="
                          background-color: #e9e9e9 !important;
                          color: #98a9bc;
                          font-size: 14px !important;
                          width: 32px;
                          margin-left: -10px;
                          height: 32px;
                        "
                      />
                    </AvatarGroup>
                  </div>
                </td>
                <td
                  class="fixcol left-350 p-3"
                  style="
                    border: 1px solid #e9e9e9;
                    background-color: #f8f9fa;
                    text-align: center;
                  "
                >
                  {{
                    l.start_date
                      ? moment(new Date(l.start_date)).format(
                          "DD/MM/YYYY HH:mm",
                        )
                      : ""
                  }}
                </td>
                <td
                  class="fixcol left-450 p-3"
                  style="
                    border: 1px solid #e9e9e9;
                    background-color: #f8f9fa;
                    text-align: center;
                  "
                >
                  {{
                    l.end_date
                      ? moment(new Date(l.end_date)).format("DD/MM/YYYY HH:mm")
                      : ""
                  }}
                </td>
                <td
                  class="no-fixcol-hover"
                  style="background-color: #fff; border: 1px solid #e9e9e9"
                  width="40"
                  :colspan="g.IsCheck ? l.totalDay : 1"
                  :style="
                    (g.Name
                      ? 'background-color: #fff;'
                      : 'background-color:' + g.bg + ';',
                    'color:' + g.color)
                  "
                  v-for="g in l.dateArray"
                >
                  <div
                    v-if="g.Name"
                    class="divbg"
                    :style="
                      'background-color:' +
                      l.status_bg_color +
                      '!important;color:' +
                      l.status_text_color
                    "
                  >
                    {{ g.Name }}
                  </div>
                </td>
              </tr>
              <tr v-if="listTask.length == 0">
                <td
                  :colspan="GrandsDate.length + 4"
                  style="text-align: center"
                >
                  <div
                    class="align-items-center justify-content-center p-4 text-center m-auto"
                    style="
                      min-height: calc(100vh - 215px);
                      max-height: calc(100vh - 215px);
                      display: flex;
                      flex-direction: column;
                    "
                    v-if="listTask != null || opition.totalRecords == 0"
                  >
                    <img
                      src="../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
    <!-- end -->
    <!-- kiểu User -->
    <div
      id="task-gantt"
      v-if="opition.type_view == 5"
      style="
        /* height: 89%; */
        max-height: calc(100vh - 500px);
        min-height: calc(100vh - 150px);
        display: -webkit-box;
        overflow-x: auto;
        overflow-y: hidden;
      "
      class="grid formgrid m-2"
    >
      <div
        class="field col-12 md:col-12"
        style="
          display: flex;
          padding: 0px;
          max-height: calc(100vh - 500px);
          min-height: calc(100vh - 150px);
        "
      >
        <div
          class="col-12 scrollbox_delayed"
          style="height: 100%; padding: 0px; overflow: auto"
        >
          <table
            class="table table-border"
            style="
              width: max-content;
              table-layout: fixed;
              min-width: 100%;
              border-collapse: collapse;
              overflow-x: scroll;
            "
          >
            <thead
              style="
                background-color: #f8f9fa;
                position: sticky;
                top: 0px;
                z-index: 5;
              "
            >
              <tr>
                <th
                  class="fixcol left-0 p-3"
                  rowspan="3"
                  style="width: 200px; border: 1px solid #e9e9e9"
                >
                  <div
                    style="
                      display: flex;
                      justify-content: center;
                      flex-direction: column;
                      align-items: center;
                    "
                  >
                    <span style="margin-bottom: 5px">
                      Thành viên {{ "(" + listDropdownUser.length + ")" }}
                    </span>
                    <span>
                      <AvatarGroup>
                        <div
                          v-for="(value, index) in listThanhVien"
                          :key="index"
                        >
                          <div>
                            <Avatar
                              v-bind:label="
                                value.avatar
                                  ? ''
                                  : (value.ten ?? '').substring(0, 1)
                              "
                              v-bind:image="basedomainURL + value.avatar"
                              style="
                                background-color: #2196f3;
                                color: #ffffff;
                                width: 32px;
                                height: 32px;
                                font-size: 15px !important;
                                margin-left: -10px;
                              "
                              :style="{
                                background: bgColor[index % 7] + '!important',
                              }"
                              class="cursor-pointer"
                              size="xlarge"
                              shape="circle"
                            />
                          </div>
                        </div>
                        <Avatar
                          v-if="
                            listDropdownUser.length - listThanhVien.length > 0
                          "
                          :label="
                            '+' +
                            (listDropdownUser.length - listThanhVien.length) +
                            ''
                          "
                          class="cursor-pointer"
                          shape="circle"
                          style="
                            background-color: #e9e9e9 !important;
                            color: #98a9bc;
                            font-size: 14px !important;
                            width: 32px;
                            margin-left: -10px;
                            height: 32px;
                          "
                        />
                      </AvatarGroup>
                    </span>
                  </div>
                </th>
                <th
                  v-for="(m, index) in Grands"
                  :key="index"
                  class="p-3"
                  align="center"
                  :width="m.Dates.length * 40"
                  :colspan="m.Dates.length"
                  style="text-align: center; min-width: 100px; color: #2196f3"
                >
                  Tháng {{ m.Month }}/{{ m.Year }}
                </th>
              </tr>
              <tr>
                <th
                  class="no-fixcol p-3"
                  width="40"
                  style="border: 1px solid #e9e9e9"
                  :style="
                    (g.bg == ''
                      ? 'background-color: #fff;'
                      : 'background-color:' + g.bg + ';',
                    'color:' + g.color)
                  "
                  v-for="(g, index) in GrandsDate"
                  :key="index"
                >
                  {{ g.DayName }}
                </th>
              </tr>
              <tr>
                <th
                  class="no-fixcol p-3"
                  width="40"
                  style="border: 1px solid #e9e9e9"
                  :style="
                    (g.bg == ''
                      ? 'background-color: #fff;'
                      : 'background-color:' + g.bg + ';',
                    'color:' + g.color)
                  "
                  v-for="(g, index) in GrandsDate"
                  :key="index"
                >
                  {{ g.DayN }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="(l, index) in listTask"
                :key="index"
              >
                <td
                  class="fixcol left-0 p-3"
                  style="
                    height: 40px;
                    border: 1px solid #dedede;
                    background-color: #f8f9fa;
                  "
                  v-if="l.IsHienThi"
                  :rowspan="l.count_cv"
                >
                  <div
                    style="display: flex; align-items: center; padding: 10px"
                  >
                    <span>
                      <Avatar
                        v-tooltip.bottom="{
                          value:
                            l.user_name +
                            '<br/>' +
                            (l.tenChucVu || '') +
                            '<br/>' +
                            (l.tenToChuc || ''),
                          escape: true,
                        }"
                        v-bind:label="
                          l.avatar ? '' : (l.last_name ?? '').substring(0, 1)
                        "
                        v-bind:image="basedomainURL + l.avatar"
                        style="
                          background-color: #2196f3;
                          color: #ffffff;
                          width: 3.5rem;
                          height: 3.5rem;
                          font-size: 15px !important;
                        "
                        :style="{
                          background: bgColor[(index % 7) + 1] + '!important',
                        }"
                        class="cursor-pointer"
                        size="xlarge"
                        shape="circle"
                      />
                    </span>
                    <span style="margin-left: 10px">
                      <div
                        style="
                          display: flex;
                          flex-direction: column;
                          line-height: 20px;
                        "
                      >
                        <b style="font-size: 13px">{{ l.user_name }}</b>
                        <div
                          style="
                            font-weight: 600;
                            color: #72777a;
                            font-size: 12px;
                          "
                        >
                          {{ l.tenChucVu }}
                        </div>
                        <div style="font-weight: 500; font-size: 11px">
                          {{ l.tenToChuc }}
                        </div>
                        <div style="display: flex">
                          <span
                            v-if="l.count_istype_0 > 0"
                            style="
                              background-color: #337ab7;
                              color: #ffffff;
                              display: inline;
                              padding: 0.4em 0.6em;
                              font-size: 75%;
                              font-weight: 700;
                              line-height: 1;
                              color: #fff;
                              text-align: center;
                              white-space: nowrap;
                              vertical-align: baseline;
                              border-radius: 0.25em;
                              margin-left: 10px;
                            "
                            >Quản lý {{ l.count_istype_0 }}</span
                          >
                          <span
                            v-if="l.count_istype_1 > 0"
                            style="
                              background-color: #5cb85c;
                              color: #ffffff;
                              display: inline;
                              padding: 0.4em 0.6em;
                              font-size: 75%;
                              font-weight: 700;
                              line-height: 1;
                              color: #fff;
                              text-align: center;
                              white-space: nowrap;
                              vertical-align: baseline;
                              border-radius: 0.25em;
                              margin-left: 5px;
                            "
                            >Thực hiện {{ l.count_istype_1 }}</span
                          >
                          <span
                            v-if="l.count_istype_3 > 0"
                            style="
                              background-color: #5bc0de;
                              color: #ffffff;
                              display: inline;
                              padding: 0.4em 0.6em;
                              font-size: 75%;
                              font-weight: 700;
                              line-height: 1;
                              color: #fff;
                              text-align: center;
                              white-space: nowrap;
                              vertical-align: baseline;
                              border-radius: 0.25em;
                              margin-left: 5px;
                            "
                            >Theo dõi {{ l.count_istype_3 }}</span
                          >
                        </div>
                      </div>
                    </span>
                  </div>
                </td>
                <td
                  @click="onRowSelect(l)"
                  rowspan="1"
                  class="no-fixcol-hover"
                  style="
                    background-color: #fff;
                    border: 1px solid #e9e9e9;
                    height: 40px;
                  "
                  width="40"
                  :colspan="g.IsCheck ? l.totalDay : 1"
                  :style="
                    (g.Name
                      ? 'background-color: #fff;'
                      : 'background-color:' + g.bg + ';',
                    'color:' + g.color)
                  "
                  v-for="(g, index) in l.dateArray"
                  :key="index"
                >
                  <div
                    v-if="g.Name"
                    class="divbg"
                    :style="
                      'background-color:' +
                      l.time_bg +
                      '!important;color:' +
                      l.status_text_color
                    "
                  >
                    {{ g.Name }}
                  </div>
                </td>
              </tr>
              <tr v-if="listTask.length == 0">
                <td
                  :colspan="GrandsDate.length + 4"
                  style="text-align: center"
                >
                  <div
                    class="align-items-center justify-content-center p-4 text-center m-auto"
                    style="
                      min-height: calc(100vh - 215px);
                      max-height: calc(100vh - 215px);
                      display: flex;
                      flex-direction: column;
                    "
                    v-if="listTask != null || opition.totalRecords == 0"
                  >
                    <img
                      src="../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
    <!-- end -->

    <DetailedWork
      v-if="showDetail === true"
      :id="selectedTaskID"
      :turn="0"
      :closeDetail="closeDetail"
    >
    </DetailedWork>
  </div>
  <DialogTask
    v-if="displayTask == true"
    :header="headerAddTask"
    :visible="displayTask"
    :is_add="is_Add"
    :is_template="is_Template"
    :data="DialogData"
    :closeDialogTask="closeDialogTask"
    :afterSave="afterSave"
  >
  </DialogTask>
</template>
<style lang="scss" scoped>
#toolbar_right .active {
  background-color: #2196f3 !important;
  border: 1px solid #5ca7e3 !important;
  color: #fff;
}

::-webkit-scrollbar {
  width: 20px;
}

.p-multiselect-label-container {
  display: flex;
  align-items: center;
}

.p-treeselect-items-wrapper {
  max-height: 200px !important;
  min-width: calc(100vw - 1100px);
  max-width: calc(100vw - 1100px);
}

.p-fileupload {
  width: 100%;
}

#btn_huy:hover {
  background-color: #eee !important;
  border-color: #eee;
}

/* task_filter */
/* #task_filter{
    width: 500px;
    height: 500px;
} */
.p-overlaypanel .p-overlaypanel-content {
  padding: 5px;
}

#task_filter .p-menuitem {
  padding: 5px 10px;
  list-style: none;
}

#task_filter .parent:hover {
  cursor: pointer;
  background-color: #e9ecef;
}

#task_filter .parent .active {
  color: #2196f3 !important;
}

#task_filter .children .active {
  color: #2196f3 !important;
}

.duan:hover {
  cursor: pointer;
  text-decoration: underline;
}

.p-progressbar {
  font-size: 10px;
  height: 1.2rem !important;
}

.p-chip {
  border-radius: 5px !important;
}

.add-user {
  padding: 7px 0px;
  outline: none;
  border: none;
}

.p-fileupload {
  margin-top: 5px;
}

#chitiet .title-lable {
  padding: 0px !important;
}

#task_sort {
  min-width: fit-content !important;
}

#header_bottom .active {
  background-color: #f18636 !important;
}

#header_bottom li {
  list-style: none;
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 30px;
  border: 1px solid #dadbdc;
  background-color: #0078d4;
  color: #fff;
}

#header_bottom li a {
  padding: 0px 10px;
}

#header_bottom li:hover {
  cursor: pointer;
  background-color: #f18636 !important;
  border: 1px solid #f18636 !important;
  color: #fff;
}

#header_bottom li a i {
  padding-right: 5px;
}

.p-tooltip-text {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.p-treeselect-panel {
  max-width: 28vw !important;
}

.p-treeselect-panel .p-treeselect-items-wrapper .p-tree {
  max-height: 17vh !important;
}

.p-dropdown-item {
  white-space: normal !important;
}

#toolbar_right li {
  list-style: none;
  display: flex;
  justify-content: center;
  align-items: center;
  height: 30px;
  border: 1px solid;
  border-radius: 4px;
  margin: 0px 5px 0px 0px;
}

#toolbar_right li a {
  padding: 0px 10px;
}

#toolbar_right li:hover {
  cursor: pointer;
  background-color: #2196f3 !important;
  border: 1px solid #5ca7e3 !important;
  color: #fff;
}
</style>
<style>
#task_filter .p-calendar-w-btn .p-inputtext {
  display: none !important;
}

#task_file .p-toolbar-group-left {
  width: 85% !important;
}

#task_sort .p-menuitem {
  padding: 5px 10px;
}

#task_sort .p-menuitem:hover {
  cursor: pointer;
  background-color: #e9ecef;
}

#task_sort .p-menuitem .active {
  color: #2196f3 !important;
}

#task_list_type .p-menuitem div {
  padding: 5px 10px;
  display: flex;
  align-items: center;
}

#task_list_type .p-menuitem:hover {
  cursor: pointer;
  background-color: #e9ecef;
}

#task_list_type .p-menuitem .active {
  color: #2196f3 !important;
}

#task_list_type .p-menuitem i {
  margin-right: 5px;
}

#task_group_list_type .p-menuitem div {
  padding: 5px 10px;
  display: flex;
  align-items: center;
}

#task_group_list_type .p-menuitem:hover {
  cursor: pointer;
  background-color: #e9ecef;
}

#task_group_list_type .p-menuitem .active {
  color: #2196f3 !important;
}

#task_group_list_type .p-menuitem i {
  margin-right: 5px;
}

#task {
  height: 89% !important;
}

#task-tree {
  height: 89% !important;
}

.p-card .p-card-title {
  font-size: 14px !important;
  text-align: center;
}

.p-card .p-card-content {
  text-align: center;
  display: flex;
  flex-direction: column;
}

.p-card .p-card-content > span {
  margin-top: 10px;
}

.p-card .p-card-body {
  padding: 1rem 0 0 0 !important;
}

.p-card .p-card-body .p-card-title,
.p-card .p-card-body .p-card-content {
  padding: 0rem 1rem;
  position: relative;
}

#task-grid .p-progressbar {
  height: 1rem !important;
}

#task-grid .p-progressbar .p-progressbar-label {
  line-height: 1rem !important;
}

#task-gantt .table tbody tr:hover {
  cursor: pointer;
  /* background-color: #E5F3FF !important; */
}

#task-gantt .table tbody tr:hover td.no-fixcol-hover {
  background-color: #e5f3ff !important;
}

#task-gantt .table thead tr .fixcol {
  z-index: 5;
  color: #000;
  font-weight: 600;
  position: sticky;
  /* background: #f5f5f5; */
  background-color: #f8f9fa;
  outline: 1px solid #e9e9e9;
  border: none;
  vertical-align: middle;
}

#task-gantt .table thead tr th {
  outline: 1px solid #e9e9e9;
}

#task-gantt .table tbody tr .fixcol {
  position: sticky;
  z-index: 0;
  color: #000;
  font-weight: 400;
  /* background: #f5f5f5; */
  background-color: #f8f9fa;
  outline: 1px solid #e9e9e9;
  border: none;
  vertical-align: middle;
}

#task-gantt .left-0 {
  left: 0px;
}

#task-gantt .left-200 {
  left: 200px;
}

#task-gantt .left-350 {
  left: 350px;
}

#task-gantt .left-450 {
  left: 450px;
}

.p-card .p-card-title span:hover {
  cursor: pointer;
  color: #2196f3 !important;
}

.p-card:hover .card-chucnang {
  display: flex !important;
}

.p-card:hover {
  cursor: pointer;
}

.p-card-title {
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
}

.scroll-outer {
  visibility: hidden;
}

.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
  visibility: visible;
}

.choose-user {
  color: #2196f3;
}

.choose-user:hover {
  cursor: pointer;
}

.p-column-header-content {
  justify-content: center;
}

.scrollbox_delayed:hover {
  transition: visibility 0s 0.2s;
  visibility: visible;
}

.divbg {
  text-align: center;
  padding: 5px;
  font-size: 11px;
  white-space: normal;
  position: relative;
  margin: 5px;
  position: inherit;
}
#type_group_view div span:hover {
  cursor: pointer;
}
#task-tree .task-tree-lable:hover {
  cursor: pointer;
}
#task-tree .task-tree-lable {
  position: sticky;
  z-index: 2;
  top: 0px;
}
</style>
