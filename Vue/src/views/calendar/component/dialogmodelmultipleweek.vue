<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function";
import moment from "moment";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { de } from "date-fns/locale";
import dialogmodelweek from "../component/dialogmodelweek.vue";
import treeuser from "../../../components/user/treeuser.vue";

const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const isDynamicSQL = ref(false);
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  isAdd: Boolean,
  submitted: Boolean,
  loading: Boolean,
  group: Number,
  currentweek: Object,
  weeks: Array,
  boardrooms: Array,
  departments: Array,
  users: Array,
  cars: Array,
  initData: Function,
});

//Declare
const datas = ref([]);
const temps = ref([]);
const currentweek = ref(props.currentweek);
const options = ref({
  loading: props.loading,
  is_group: props.group,
  week: currentweek.value["week_no"],
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
const types = ref([
  { is_type: 0, name: "Họp bình thường" },
  { is_type: 1, name: "Họp trực tuyến" },
]);
const iterations = ref([
  { is_iterations: 0, name: "Không lặp", short: "ngày" },
  { is_iterations: 1, name: "Lặp theo ngày", short: "ngày" },
  { is_iterations: 2, name: "Lặp theo tuần", short: "tuần" },
  { is_iterations: 3, name: "Lặp theo tháng", short: "tháng" },
  { is_iterations: 4, name: "Lặp theo năm", short: "năm" },
]);
Date.prototype.addDays = function (days) {
  var date = new Date(this.valueOf());
  date.setDate(date.getDate() + days);
  return date;
};
const getDayDate = (d) => {
  var date = new Date(d);
  var current_day = date.getDay();
  var day_name = "";
  if (current_day != null) {
    switch (current_day) {
      case 0:
        day_name = "Chủ Nhật";
        break;
      case 1:
        day_name = "Thứ Hai";
        break;
      case 2:
        day_name = "Thứ Ba";
        break;
      case 3:
        day_name = "Thứ Tư";
        break;
      case 4:
        day_name = "Thứ Năm";
        break;
      case 5:
        day_name = "Thứ Sáu";
        break;
      case 6:
        day_name = "Thứ Bảy";
        break;
      default:
        break;
    }
  }

  return day_name;
};
function isValidDate(d) {
  return d instanceof Date && !isNaN(d);
}
const bindDateBetweenFirstAndLast = (
  start_date,
  end_date,
  add_fn,
  interval
) => {
  var retVal = [];
  if (isValidDate(start_date) && isValidDate(end_date)) {
    add_fn = add_fn || Date.prototype.addDays;
    interval = interval || 1;

    var current = new Date(start_date);

    var checkVR = true;
    if (current >= end_date) {
      checkVR = false;
    }
    while (checkVR) {
      if (current >= end_date) {
        checkVR = false;
      }
      retVal.push(new Date(current));
      current = add_fn.call(current, interval);
    }
  }
  return retVal;
};
function CreateGuid() {
  function _p8(s) {
    var p = (Math.random().toString(16) + "000000000").substring(2, 8);
    return s ? "-" + p.substring(0, 4) + "-" + p.substring(4, 4) : p;
  }
  return _p8() + _p8(true) + _p8(true) + _p8();
}

//Filter
const goWeek = (week) => {
  options.value.week = week;
  currentweek.value = props.weeks.find((x) => x["week_no"] === week);
  initWeek(true);
};

//Choose file
const files = ref([]);
const onUpload = () => {};
const removeFile = (event) => {
  files.value = [];
  event.files.forEach((element) => {
    files.value.push(element);
  });
};
const selectFile = (event) => {
  files.value = [];
  event.files.forEach((element) => {
    files.value.push(element);
  });
};

//Function
const model = ref({});
const isAdd = ref(false);
const submitted = ref(props.submitted);
const headerDialogWeek = ref();
const displayDialogWeek = ref(false);
const openAddDialogWeek = (str, item) => {
  files.value = [];
  isAdd.value = true;
  submitted.value = false;
  var start_date = new Date(item["day"]);
  var end_date = new Date(item["day"]);
  model.value = {
    create_date: new Date(),
    start_date: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    start_date_time: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    end_date: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    end_date_time: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    is_group: options.value.is_group,
    status: 0,
    is_type: 0,
    is_iterations: 0,
    distance_iterations: 0,
    numeric_iterations: 0,
    numeric_attendees: 0,
    chutris: [],
    thamgias: [],
    departments: [],
    files: [],
  };
  headerDialogWeek.value = str;
  displayDialogWeek.value = true;
};
const addModelTemp = (item) => {
  var start_date = new Date(item["day"]);
  var end_date = new Date(item["day"]);
  var md = {
    calendar_id: CreateGuid(),
    create_date: new Date(),
    start_date: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    start_date_time: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    end_date: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    end_date_time: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    day: new Date(
      item["day"].getFullYear(),
      item["day"].getMonth(),
      item["day"].getDate(),
      7,
      0,
      0
    ),
    day_name: getDayDate(item["day"]),
    day_string: moment(item["day"]).format("DD/MM/YYYY"),
    is_holiday: new Date(item["day"]).getDay() == 0,
    is_group: options.value.is_group,
    status: 0,
    is_type: 0,
    is_iterations: 0,
    distance_iterations: 0,
    numeric_iterations: 0,
    numeric_attendees: 0,
    chutris: [],
    thamgias: [],
    departments: [],
    files: [],
  };

  if (props.group === 1 && leaders.value && leaders.value.length > 0) {
    leaders.value.forEach((leader) => {
      (md.calendar_id = CreateGuid()),
        (md.chutris = [
          {
            user_id: leader.user_id,
            full_name: leader.full_name,
            last_name: leader.last_name,
            avatar: leader.avatar,
            is_order: leader.is_order,
          },
        ]);
      var it = Object.assign({}, md);
      datas.value.push(it);
    });
  } else {
    datas.value.ptestush(md);
  }
  initWeek();
};
const changeContents = (md) => {
  let contents = document.getElementById("contents" + md.calendar_id);
  if (contents) {
    md.contents = contents.innerHTML;
  }
};
const closeDialogWeek = () => {
  model.value = {
    status: 0,
    is_type: 0,
    is_iterations: 0,
    distance_iterations: 0,
    numeric_iterations: 0,
    numeric_attendees: 0,
    chutris: [],
    thamgias: [],
    departments: [],
    files: [],
  };
  displayDialogWeek.value = false;
};
const saveTemp = (md) => {
  if (md["files"] != null && md["files"].length > 0) {
    md["files"].forEach((file) => {
      file["file_name"] = file["name"];
      file["file_type"] = file["type"].split("/").at(-1);
      file["file_size"] = file["size"];
    });
  }
  if (isAdd.value === true) {
    md["calendar_id"] = CreateGuid();
    var start_date = new Date(md["start_date"]);
    md["day"] = new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    );
    md["day_name"] = getDayDate(md["day"]);
    md["day_string"] = moment(md["day"]).format("DD/MM/YYYY");
    md["is_holiday"] = new Date(md["day"]).getDay() == 0;
    var checkroom = props.boardrooms.findIndex(
      (x) => x["boardroom_id"] === md["boardroom_id"]
    );
    if (checkroom === -1) {
      md["place_name"] = md["boardroom_id"];
      md["boardroom_name"] = md["place_name"];
    }
    datas.value.push(md);
  } else {
    var idx = datas.value.findIndex(
      (x) => x["calendar_id"] === md["calendar_id"]
    );
    if (idx !== -1) {
      var start_date = new Date(md["start_date"]);
      md["day"] = new Date(
        start_date.getFullYear(),
        start_date.getMonth(),
        start_date.getDate(),
        7,
        0,
        0
      );
      md["day_name"] = getDayDate(md["day"]);
      md["day_string"] = moment(md["day"]).format("DD/MM/YYYY");
      md["is_holiday"] = new Date(md["day"]).getDay() == 0;
      var checkroom = props.boardrooms.findIndex(
        (x) => x["boardroom_id"] === md["boardroom_id"]
      );
      if (checkroom === -1) {
        md["place_name"] = md["boardroom_id"];
        md["boardroom_name"] = md["place_name"];
      }
      datas.value[idx] = md;
    }
  }
  closeDialogWeek();
  initWeek();
};
const openEditDialogWeek = (md) => {
  forceRerender(2);
  files.value = [];
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isAdd.value = false;
  var contents = document.getElementById("contents" + md.calendar_id);
  if (contents) {
    md.contents = contents.innerHTML;
  }
  if (md["boardroom_id"] == null && md["place_name"] != null) {
    md["boardroom_id"] = md["place_name"];
  }
  model.value = md;
  swal.close();
  headerDialogWeek.value = "Cập nhật lịch họp";
  displayDialogWeek.value = true;
};
const deleteItem = (md) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var idx = datas.value.findIndex(
    (x) => x["calendar_id"] === md["calendar_id"]
  );
  if (idx !== -1) {
    datas.value.splice(idx, 1);
  }
  initWeek();
  swal.close();
};
const saveModelMultiple = () => {
  if (datas.value == null || datas.value.length === 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng thêm mới lịch họp!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  var invalid = false;
  if (datas.value != null && datas.value.length > 0) {
    datas.value.forEach((item, i) => {
      var checkroom = props.boardrooms.findIndex(
        (x) => x["boardroom_id"] === (item["boardroom_id"] || "")
      );
      if (checkroom === -1) {
        item["place_name"] = item["boardroom_id"] || "";
        item["boardroom_name"] = item["place_name"];
        item["boardroom_id"] = null;
      }
      var contents = document.getElementById("contents" + item["calendar_id"]);
      if (contents) {
        item["contents"] = contents.innerHTML;
      }
      if (
        item["contents"] == null ||
        (item["boardroom_id"] == null && item["place_name"] == null)
      ) {
        invalid = true;
        return;
      }
    });
  }
  if (invalid) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng nhập đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formData = new FormData();
  var multiple = [...datas.value];
  var members = [];
  var departments = [];
  multiple.forEach((item) => {
    //Date
    if (item["start_date"] != null) {
      item["start_date"] = moment(item["start_date"]).format(
        "YYYY-MM-DDTHH:mm:ssZZ"
      );
    }
    if (item["end_date"] != null) {
      item["end_date"] = moment(item["end_date"]).format(
        "YYYY-MM-DDTHH:mm:ssZZ"
      );
    }
    if (item["create_date"] != null) {
      item["create_date"] = moment(item["create_date"]).format(
        "YYYY-MM-DDTHH:mm:ssZZ"
      );
    }
    //Chủ trì
    if (item["chutris"] != null && item["chutris"].length > 0) {
      item["chutris"].forEach((mb) => {
        var obj = {
          calendar_id: item["calendar_id"],
          user_id: mb["user_id"],
          is_type: 0,
        };
        members.push(obj);
      });
    }
    //Thành phần tham gia
    if (item["thamgias"] != null && item["thamgias"].length > 0) {
      item["thamgias"].forEach((mb) => {
        var obj = {
          calendar_id: item["calendar_id"],
          user_id: mb["user_id"],
          is_type: 1,
        };
        members.push(obj);
      });
    }
    //Phòng ban
    if (item["departments"] != null && item["departments"].length > 0) {
      item["departments"].forEach((dp) => {
        var obj = {
          calendar_id: item["calendar_id"],
          department_id: dp,
        };
        departments.push(obj);
      });
    }
    //Files
    if (item["files"] != null && item["files"].length > 0) {
      for (var i = 0; i < item["files"].length; i++) {
        let file = item["files"][i];
        formData.append("files" + item["calendar_id"], file);
      }
    }
  });
  formData.append("multiple", JSON.stringify(multiple));
  formData.append("members", JSON.stringify(members));
  formData.append("departments", JSON.stringify(departments));
  axios
    .put(
      baseURL + "/api/calendar_week/update_calendar_week_multiple",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err === "1") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
      swal.close();
      toast.success("Thêm lịch họp thành công!");
      props.closeDialog();
      props.initData(true);
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
  if (submitted.value) submitted.value = true;
};

//Function choice user
// reload component
var calendar_id_public = null;
const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value) {
    componentKey.value = { type: 0 };
  }
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};
const selectedUser = ref([]);
const is_one = ref(false);
const is_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const showModalUser = (model, one, type) => {
  if (type != null) {
    calendar_id_public = model["calendar_id"];
    switch (type) {
      case 0:
        selectedUser.value = [...model.chutris];
        headerDialogUser.value = "Chọn người chủ trì";
        break;
      case 1:
        selectedUser.value = [...model.thamgias];
        headerDialogUser.value = "Chọn người tham gia";
        break;
      default:
        break;
    }
  }

  is_one.value = one;
  is_type.value = type;
  displayDialogUser.value = true;
  forceRerender(1);
};
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
const choiceUser = () => {
  var model = null;
  var idx = datas.value.findIndex((x) => x.calendar_id === calendar_id_public);
  if (idx != -1) {
    model = datas.value.at(idx);
  }
  if (model != null) {
    if (is_type.value != null) {
      switch (is_type.value) {
        case 0:
          var notexist = selectedUser.value.filter(
            (a) =>
              model.chutris.findIndex((b) => b["user_id"] === a["user_id"]) ===
              -1
          );
          if (notexist.length > 0) {
            model.chutris = notexist;
          }
          break;
        case 1:
          var notexist = selectedUser.value.filter(
            (a) =>
              model.thamgias.findIndex((b) => b["user_id"] === a["user_id"]) ===
              -1
          );
          if (notexist.length > 0) {
            model.thamgias = model.thamgias.concat(notexist);
          }
          break;
        default:
          break;
      }
    }
    model.numeric_attendees = model.chutris.length + model.thamgias.length;
  }
  closeDialogUser();
};

//Carousel
const page = ref();
const numVisible = ref(9);
const numScroll = ref(8);
var width = window.screen.width;
if (width > 1400) {
  numVisible.value = 9;
  numScroll.value = 8;
} else if (width > 1300) {
  numVisible.value = 8;
  numScroll.value = 7;
} else if (width > 1200) {
  numVisible.value = 7;
  numScroll.value = 6;
} else if (width > 1100) {
  numVisible.value = 6;
  numScroll.value = 6;
} else if (width > 1000) {
  numVisible.value = 5;
  numScroll.value = 5;
} else if (width > 900) {
  numVisible.value = 4;
  numScroll.value = 4;
} else if (width > 800) {
  numVisible.value = 3;
  numScroll.value = 3;
} else if (width > 700) {
  numVisible.value = 3;
  numScroll.value = 3;
} else if (width > 600) {
  numVisible.value = 2;
  numScroll.value = 2;
} else if (width > 500) {
  numVisible.value = 2;
  numScroll.value = 2;
} else if (width > 400) {
  numVisible.value = 1;
  numScroll.value = 1;
} else {
  numScroll.value = 1;
}
page.value = Math.floor(numScroll.value * (currentweek.value.week_no / 52));

//init week
const leaders = ref([]);
const initLeader = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_leader_get",
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
          if (tbs[0] != null && tbs[0].length > 0) {
            leaders.value = tbs[0];
          } else {
            leaders.value = [];
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const initWeek = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  temps.value = [];
  setTimeout(() => {
    if (datas.value && datas.value.length > 0) {
      var start_date = new Date(currentweek.value["week_start_date"]);
      var end_date = new Date(currentweek.value["week_end_date"]);
      temps.value = datas.value.filter(
        (item) =>
          item["start_date"] >= start_date &&
          item["start_date"] <= end_date.setDate(end_date.getDate() + 1)
      );
    }
    let dateinweeks = bindDateBetweenFirstAndLast(
      new Date(currentweek.value["week_start_date"]),
      new Date(currentweek.value["week_end_date"])
    );
    dateinweeks
      .filter(
        (a) =>
          temps.value.findIndex(
            (b) => b["day_string"] === moment(a).format("DD/MM/YYYY")
          ) === -1
      )
      .forEach((day, i) => {
        temps.value.push({
          day: day,
          day_name: getDayDate(day),
          day_string: moment(day).format("DD/MM/YYYY"),
          is_holiday: day.getDay() == 0,
        });
      });
    temps.value = temps.value.sort(function (a, b) {
      return (
        new Date(a["start_date"] || a["day"]) -
        new Date(b["start_date"] || b["day"])
      );
    });
    swal.close();
    if (options.value.loading) options.value.loading = false;
  }, 100);
};
onMounted(() => {
  initLeader();
  initWeek(true);
  window.addEventListener("resize", (event) => {
    var width = window.screen.width;
    if (width > 1400) {
      numVisible.value = 9;
      numScroll.value = 8;
    } else if (width > 1300) {
      numVisible.value = 8;
      numScroll.value = 7;
    } else if (width > 1200) {
      numVisible.value = 7;
      numScroll.value = 6;
    } else if (width > 1100) {
      numVisible.value = 6;
      numScroll.value = 6;
    } else if (width > 1000) {
      numVisible.value = 5;
      numScroll.value = 5;
    } else if (width > 900) {
      numVisible.value = 4;
      numScroll.value = 4;
    } else if (width > 800) {
      numVisible.value = 3;
      numScroll.value = 3;
    } else if (width > 700) {
      numVisible.value = 3;
      numScroll.value = 3;
    } else if (width > 600) {
      numVisible.value = 2;
      numScroll.value = 2;
    } else if (width > 500) {
      numVisible.value = 2;
      numScroll.value = 2;
    } else if (width > 400) {
      numVisible.value = 1;
      numScroll.value = 1;
    } else {
      numScroll.value = 1;
    }
    page.value = Math.floor(numScroll.value * (currentweek.value.week_no / 52));
  });
  return {};
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '80vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 1000"
  >
    <form>
      <div class="row m-2">
        <div class="col-12 md:col-12">
          <Carousel
            :value="weeks"
            :page="page"
            :numVisible="numVisible"
            :numScroll="numScroll"
            class="custom-carousel"
            :circular="false"
            :responsiveOptions="[
              {
                breakpoint: '1400px',
                numVisible: 9,
                numScroll: 8,
              },
              {
                breakpoint: '1300px',
                numVisible: 8,
                numScroll: 7,
              },
              {
                breakpoint: '1200px',
                numVisible: 7,
                numScroll: 6,
              },
              {
                breakpoint: '1100px',
                numVisible: 6,
                numScroll: 5,
              },
              {
                breakpoint: '1000px',
                numVisible: 5,
                numScroll: 4,
              },
              {
                breakpoint: '900px',
                numVisible: 4,
                numScroll: 3,
              },
              {
                breakpoint: '800px',
                numVisible: 3,
                numScroll: 2,
              },
              {
                breakpoint: '700px',
                numVisible: 3,
                numScroll: 1,
              },
              {
                breakpoint: '600px',
                numVisible: 2,
                numScroll: 1,
              },
              {
                breakpoint: '500px',
                numVisible: 2,
                numScroll: 1,
              },
              {
                breakpoint: '400px',
                numVisible: 1,
                numScroll: 1,
              },
            ]"
          >
            <template #header>
              <h4 class="mt-0">Chọn tuần đặt lịch</h4>
            </template>
            <template #item="slotProps">
              <div
                @click="goWeek(slotProps.data.week_no)"
                class="format-grid-center item-week"
                :class="{
                  pass: slotProps.data.pass,
                  current: slotProps.data.week_no === options.week,
                }"
              >
                <b class="mb-2">Tuần {{ slotProps.data.week_no }}</b>
                <div>
                  {{
                    moment(new Date(slotProps.data.week_start_date)).format(
                      "DD/MM/YYYY"
                    )
                  }}
                </div>
                <div>
                  {{
                    moment(new Date(slotProps.data.week_end_date)).format(
                      "DD/MM/YYYY"
                    )
                  }}
                </div>
              </div>
            </template>
          </Carousel>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Chi tiết đặt lịch họp tuần</label>
            <div class="lang-table">
              <DataTable
                @sort="onSort($event)"
                :value="temps"
                :lazy="true"
                :rowHover="true"
                :showGridlines="true"
                :scrollable="false"
                v-model:selection="selectedNodes"
                dataKey="calendar_id"
                scrollHeight="flex"
                filterDisplay="menu"
                filterMode="lenient"
                rowGroupMode="rowspan"
                groupRowsBy="day_string"
              >
                <Column
                  field="day_string"
                  header=""
                  headerStyle="text-align:center;width:50px;height:50px;"
                  bodyStyle="text-align:center;width:50px;max-height:60px"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <div class="format-center">
                      <Button
                        @click="
                          //openAddDialogWeek('Thêm mới lịch họp', slotProps.data)
                          addModelTemp(slotProps.data)
                        "
                        class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                        type="button"
                        icon="pi pi-plus-circle"
                        v-tooltip.top="'Thêm lịch họp'"
                      ></Button>
                    </div>
                  </template>
                </Column>
                <Column
                  field="day_string"
                  header="Ngày tháng"
                  headerStyle="text-align:center;width:100px;height:50px;"
                  bodyStyle="text-align:center;width:100px;max-height:60px"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <div
                      class="format-grid-center style-day"
                      :class="{
                        true: slotProps.data.is_holiday,
                        false: !slotProps.data.is_holiday,
                      }"
                    >
                      <b>{{ slotProps.data.day_name }}</b>
                      <span>{{ slotProps.data.day_string }}</span>
                    </div>
                  </template>
                </Column>
                <Column
                  field="contents"
                  headerStyle="height:50px;max-width:auto;min-width:150px;"
                  bodyStyle="max-height:60px;"
                >
                  <template #header>
                    <div><b>Nội dung </b><span class="redsao">(*)</span></div>
                  </template>
                  <template #body="slotProps">
                    <div
                      v-if="slotProps.data.calendar_id != null"
                      contentEditable="true"
                      :id="'contents' + slotProps.data.calendar_id"
                      class="box-contents w-full"
                      v-html="slotProps.data.contents"
                      @input="changeContents(slotProps.data)"
                      :class="{
                        'p-invalid': !slotProps.data.contents && submitted,
                      }"
                      :style="{
                        minHeight: '50px',
                        border: 'solid 1px #ced4da',
                        borderRadius: '3px',
                        padding: '0.5rem',
                        backgroundColor: '#fff',
                      }"
                    ></div>
                  </template>
                </Column>
                <Column
                  field="chutris"
                  header="Chủ trì"
                  headerStyle="text-align:center;width:80px;height:50px"
                  bodyStyle="text-align:center;width:80px;max-height:60px"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <div class="flex justify-content-center">
                      <AvatarGroup
                        v-if="
                          slotProps.data.chutris &&
                          slotProps.data.chutris.length > 0
                        "
                        @click="showModalUser(slotProps.data, true, 0)"
                      >
                        <Avatar
                          v-for="(item, index) in slotProps.data.chutris.slice(
                            0,
                            3
                          )"
                          v-bind:label="
                            item.avatar ? '' : item.last_name.substring(0, 1)
                          "
                          v-bind:image="
                            item.avatar
                              ? basedomainURL + item.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          v-tooltip.top="item.full_name"
                          :key="item.user_id"
                          style="border: 2px solid orange; color: white"
                          @click="onTaskUserFilter(item)"
                          @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                          size="large"
                          shape="circle"
                          class="cursor-pointer"
                          :style="{ backgroundColor: bgColor[index % 7] }"
                        />
                        <Avatar
                          v-if="
                            slotProps.data.chutris &&
                            slotProps.data.chutris.length > 3
                          "
                          v-bind:label="
                            '+' + (slotProps.data.chutris.length - 3).toString()
                          "
                          shape="circle"
                          size="large"
                          style="background-color: #2196f3; color: #ffffff"
                          class="cursor-pointer"
                        />
                      </AvatarGroup>
                      <div
                        v-else-if="slotProps.data.calendar_id != null"
                        class="format-center"
                      >
                        <Button
                          @click="showModalUser(slotProps.data, true, 0)"
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                          type="button"
                          icon="pi pi-plus-circle"
                          v-tooltip.top="'Thêm người chủ trì'"
                        ></Button>
                      </div>
                    </div>
                  </template>
                </Column>
                <Column
                  field="thamgias"
                  header="Thành viên tham gia"
                  headerStyle="text-align:center;width:150px;height:50px"
                  bodyStyle="text-align:center;width:150px;max-height:60px"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <div class="flex justify-content-center">
                      <AvatarGroup
                        v-if="
                          slotProps.data.thamgias &&
                          slotProps.data.thamgias.length > 0
                        "
                        @click="showModalUser(slotProps.data, false, 1)"
                      >
                        <Avatar
                          v-for="(item, index) in slotProps.data.thamgias.slice(
                            0,
                            3
                          )"
                          v-bind:label="
                            item.avatar ? '' : item.last_name.substring(0, 1)
                          "
                          v-bind:image="
                            item.avatar
                              ? basedomainURL + item.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          v-tooltip.top="item.full_name"
                          :key="item.user_id"
                          style="border: 2px solid white; color: white"
                          @click="onTaskUserFilter(item)"
                          @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                          size="large"
                          shape="circle"
                          class="cursor-pointer"
                          :style="{ backgroundColor: bgColor[index % 7] }"
                        />
                        <Avatar
                          v-if="
                            slotProps.data.thamgias &&
                            slotProps.data.thamgias.length > 3
                          "
                          v-bind:label="
                            '+' +
                            (slotProps.data.thamgias.length - 3).toString()
                          "
                          shape="circle"
                          size="large"
                          style="background-color: #2196f3; color: #ffffff"
                          class="cursor-pointer"
                        />
                      </AvatarGroup>
                      <div
                        v-else-if="slotProps.data.calendar_id != null"
                        class="format-center"
                      >
                        <Button
                          @click="showModalUser(slotProps.data, false, 1)"
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                          type="button"
                          icon="pi pi-plus-circle"
                          v-tooltip.top="'Thêm người tham gia'"
                        ></Button>
                      </div>
                    </div>
                    <div class="mt-2" v-if="slotProps.data.invitee">
                      Người được mời: {{ slotProps.data.invitee }}
                    </div>
                    <div
                      class="mt-2"
                      v-if="
                        slotProps.data.departments &&
                        slotProps.data.departments.length > 0
                      "
                    >
                      <div>
                        Phòng ban được mời:
                        <span
                          v-for="(item, index) in slotProps.data.departments"
                          :key="index"
                        >
                          <span
                            v-if="
                              index > 0 &&
                              index < slotProps.data.departments.length
                            "
                            >,
                          </span>
                          {{ item.department_name }}
                        </span>
                      </div>
                    </div>
                  </template>
                </Column>
                <Column
                  v-if="props.group === 1"
                  field="car_name"
                  header="Xe sử dụng"
                  headerStyle="text-align:center;width:100px;height:50px"
                  bodyStyle="text-align:center;width:100px;max-height:60px"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <Dropdown
                      v-if="slotProps.data.calendar_id != null"
                      :options="props.cars"
                      :filter="true"
                      :showClear="true"
                      :editable="false"
                      v-model="slotProps.data.car_id"
                      optionLabel="car_name"
                      optionValue="car_id"
                      placeholder="Chọn xe"
                      class="ip36"
                    >
                      <template #option="slotProps">
                        <div
                          v-if="slotProps.option"
                          class="country-item flex align-items-center"
                        >
                          <div class="pt-1 pl-2">
                            <div>
                              <b>{{ slotProps.option.car_code }}</b
                              ><span> / {{ slotProps.option.car_name }}</span>
                            </div>
                          </div>
                        </div>
                      </template>
                    </Dropdown>
                  </template>
                </Column>
                <Column
                  field="boardroom_name"
                  headerStyle="text-align:center;width:150px;height:50px"
                  bodyStyle="text-align:center;width:150px;max-height:60px"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #header>
                    <div>
                      <b
                        >{{
                          props.group === 0
                            ? "Địa điểm họp"
                            : props.group === 1
                            ? "Địa điểm công tác"
                            : ""
                        }} </b
                      ><span v-if="props.group === 0" class="redsao"> (*)</span>
                    </div>
                  </template>
                  <template #body="slotProps">
                    <div v-if="props.group === 0">
                      <Dropdown
                        v-if="slotProps.data.calendar_id != null"
                        :options="props.boardrooms"
                        :filter="true"
                        :showClear="true"
                        :editable="true"
                        :class="{
                          'p-invalid':
                            !slotProps.data.boardroom_id &&
                            !slotProps.data.place_name &&
                            submitted,
                        }"
                        v-model="slotProps.data.boardroom_id"
                        optionLabel="boardroom_name"
                        optionValue="boardroom_id"
                        placeholder="Chọn phòng họp"
                        class="ip36"
                      >
                        <template #option="slotProps">
                          <div class="country-item flex align-items-center">
                            <div class="pt-1 pl-2">
                              {{ slotProps.option.boardroom_name }}
                            </div>
                          </div>
                        </template>
                      </Dropdown>
                    </div>
                    <div v-if="props.group === 1">
                      <InputText
                        v-if="slotProps.data.calendar_id != null"
                        v-model="slotProps.data.boardroom_id"
                        spellcheck="false"
                        :placeholder="
                          props.group === 0
                            ? 'Địa điểm họp'
                            : props.group === 1
                            ? 'Địa điểm công tác'
                            : ''
                        "
                        class="ip36"
                      />
                    </div>
                  </template>
                </Column>
                <Column
                  v-if="props.group === 0"
                  field="equip"
                  header="Chuẩn bị"
                  headerStyle="text-align:center;width:150px;height:50px"
                  bodyStyle="text-align:center;width:150px;max-height:60px"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <Textarea
                      v-if="slotProps.data.calendar_id != null"
                      v-model="slotProps.data.equip"
                      :autoResize="true"
                      rows="2"
                      style="width: 100%"
                    />
                  </template>
                </Column>
                <Column
                  header="Chức năng"
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;width:100px;height:50px"
                  bodyStyle="text-align:center;max-width:100px;max-height:60px"
                >
                  <template #body="slotProps">
                    <div>
                      <Button
                        v-if="slotProps.data.calendar_id != null"
                        @click="openEditDialogWeek(slotProps.data)"
                        class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                        type="button"
                        icon="pi pi-pencil"
                        v-tooltip.top="'Sửa'"
                      ></Button>
                      <Button
                        v-if="slotProps.data.calendar_id != null"
                        @click="deleteItem(slotProps.data)"
                        class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                        type="button"
                        v-tooltip.top="'Xóa'"
                        icon="pi pi-trash"
                      ></Button>
                    </div>
                  </template>
                </Column>
                <template #empty>
                  <div
                    v-if="!options.loading && (!isFirst || options.total == 0)"
                    class="flex align-items-center justify-content-center p-4 text-center m-auto"
                  >
                    <div style="height: calc(100vh)"></div>
                  </div>
                </template>
              </DataTable>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveModelMultiple()" />
    </template>
  </Dialog>

  <!--Model-->
  <dialogmodelweek
    :key="componentKey[2]"
    :temp="true"
    :headerDialog="headerDialogWeek"
    :displayDialog="displayDialogWeek"
    :closeDialog="closeDialogWeek"
    :isAdd="isAdd"
    :submitted="submitted"
    :loading="options.loading"
    :group="options.is_group"
    :model="model"
    :files="files"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :boardrooms="props.boardrooms"
    :departments="props.departments"
    :users="props.users"
    :cars="props.cars"
    :initData="saveTemp"
  />

  <!--treeuser-->
  <treeuser
    :key="componentKey[1]"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="is_one"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
</template>
<style scoped>
@import url(./stylecalendar.css);
</style>
<style lang="scss" scoped>
::v-deep(.lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
  }
  .p-avatar-text {
    font-size: 1rem;
  }
}
::v-deep(.avatar-item) {
  .p-avatar.p-avatar-lg {
    width: 3rem;
    height: 3rem;
  }
}
::v-deep(.is-close) {
  .p-panel-header {
    color: red;
  }
}
</style>
