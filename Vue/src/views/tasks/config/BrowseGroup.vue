<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../util/function.js";
import moment from "moment";
import treeuser from "../../../components/user/treeuser.vue";
const toast = useToast();
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
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
  "#FF88D3",
]);
let user = store.state.user;
const BrowserGroup = ref({
  group_name: null,
  group_role: null,
  user_id: [],
  user_count: 0,
  is_order: false,
  is_default: false,
  status: true,
});
const rules = {
  group_name: { required },
  user_id: { required },
};
const v$ = useVuelidate(rules, BrowserGroup);
const submitted = ref(false);
const addNew = (str) => {
  textboxLength.value = 0;
  DialogVisible.value = true;
  BrowserGroup.value = {
    group_name: null,
    group_role: 0,
    user_id: [],
    user_count: 0,
    is_order: options.value.totalRecords + 1,
    is_default: options.value.totalRecords > 0 ? false : true,
    status: true,
  };
  headerDialog.value = str;
};
const editOld = (data, str) => {
  edit.value = true;
  BrowserGroup.value = null;
  BrowserGroup.value = data;
  BrowserGroup.value.user_id = [];
  if (data.user_info != null) {
    data.user_info.forEach((x) => {
      BrowserGroup.value.user_id.push(x);
    });
  } else {
    BrowserGroup.value.user_id = [];
  }
  DialogVisible.value = true;
  headerDialog.value = str;
};
const headerDialog = ref();
const DialogVisible = ref(false);
const textboxLength = ref();
const checkSpace = ref();
let pattern = /\s/;
const checkconflix = () => {
  textboxLength.value = 0;
  const textbox = document.getElementById("group_name");
  textboxLength.value = textbox.value.length;
  checkSpace.value = pattern.test(textbox.value);
};
const group_role = ref([
  { code: 0, label: "Duyệt tuần tự", tip: "Duyệt theo thứ tự người duyệt" },
  {
    code: 1,
    label: "Duyệt một trong nhiều",
    tip: "Nhiều người duyệt. Chỉ cần một người xác nhận.",
  },
  {
    code: 2,
    label: "Duyệt ngẫu nhiên",
    tip: "Tất cả người duyệt cần xác nhận.",
  },
]);
//usersTree from devPhien
const componentKey = ref(0);
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const is_one = ref(false);
const selectedUser = ref([]);
const is_type = ref();
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
const choiceUser = () => {
  switch (is_type.value) {
    default:
    case 0:
      var notexist = selectedUser.value.filter(
        (a) =>
          BrowserGroup.value.user_id.findIndex(
            (b) => b["user_id"] === a["user_id"],
          ) === -1,
      );
      if (notexist.length > 0) {
        BrowserGroup.value.user_id =
          BrowserGroup.value.user_id.concat(notexist);
      }
      break;
  }
  closeDialogUser();
};
const selectUser = (one, type) => {
  switch (type) {
    default:
    case 0:
      selectedUser.value = [...BrowserGroup.value.user_id];
      headerDialogUser.value = "Chọn người duyệt";
      break;
  }
  is_one.value = one;
  is_type.value = type;
  displayDialogUser.value = true;
  forceRerender();
};
const forceRerender = () => {
  componentKey.value += 1;
};
const removeUser = (item, us) => {
  var idx = us.findIndex((x) => x["user_id"] === item["user_id"]);
  if (idx != -1) {
    us.splice(idx, 1);
  }
};
// hết userTree DevPhien
//saveDataa
const edit = ref(false);
const saveData = (isFormValid) => {
  if (textboxLength.value > 250) {
    return;
  }
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let dataSend = {
    group_name: BrowserGroup.value.group_name,
    group_role: BrowserGroup.value.group_role,
    user_id: "",
    user_count: BrowserGroup.value.user_id.length,
    is_order: BrowserGroup.value.is_order,
    is_default: BrowserGroup.value.is_default,
    status: BrowserGroup.value.status,
    organization_id: user.organization_id,
  };
  BrowserGroup.value.user_id.forEach((x) => {
    dataSend.user_id += (dataSend.user_id == "" ? "" : ",") + x.user_id;
  });
  if (edit.value == true) {
    dataSend.group_id = BrowserGroup.value.group_id;
  }
  axios({
    method: edit.value ? "put" : "post",
    url:
      baseURL +
      `/api/task_browser_group/${edit.value ? "Update_group" : "add_group"}`,
    data: dataSend,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        BrowserGroup.value = {};
        toast.success(
          response.config.method == "put"
            ? "Sửa nhóm duyệt thành công!"
            : "Thêm nhóm duyệt thành công",
        );
        swal.close();
        edit.value = false;
        loadData(true);
        DialogVisible.value = false;
        submitted.value = false;
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html: ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
//Dữ liệu
const options = ref({
  user: user.user_id,
  loading: false,
  PageNo: 0,
  PageSize: 20,
  totalRecords: 0,
});
const first = ref();
const selectedEmailGroups = ref();
const isFirst = ref(false);
const datalists = ref();
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_browse_group_count",
            par: [{ par: "user_id", va: store.state.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "BrowseGroup.vue",
        logcontent: error.message,
        loai: 2,
      });
    });
};
const loadData = (rf) => {
  first.value = 0;
  if (rf) {
    loadCount();
    axios
      .post(
        baseURL + "/api/DictionaryProc/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "task_browse_group_list ",
              par: [
                { par: "user_id", va: store.state.user.user_id },
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
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;

          element.user_info = JSON.parse(element.user_info);
          let f = group_role.value.filter((x) => x.code == element.group_role);
          element.group_role_name = f[0].label;
        });

        if (isFirst.value) isFirst.value = false;
        datalists.value = data;
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        addLog({
          title: "Lỗi Console loadData",
          controller: "BrowseGroup.vue",
          logcontent: error.message,
          loai: 2,
        });
        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo!",
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
};
const EmitData = ref();
const isShowEmail = ref(false);
const showEmails = (data) => {
  isShowEmail.value = true;
  EmitData.value = null;
  DialogVisible.value = false;
  EmitData.value = data;
};
const hideall = () => {
  isShowEmail.value = false;
  loadData(true);
};
watch(selectedEmailGroups, () => {
  if (selectedEmailGroups.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const checkDelList = ref();
const onCheckBox = (value, num) => {
  if (num == 1 && value.is_default == true) {
    swal.fire({
      title: "Thông báo!",
      text: "Cần ít nhất một nhóm duyệt mặc định!",
      icon: "error",
      confirmButtonText: "OK",
    });
    loadData(true);
    return;
  }
  //0 cập nhật stt, 1 cập nhật default
  if (num == 0 && value.is_default == true) {
    swal.fire({
      title: "Thông báo!",
      text: "Nhóm duyệt mặc định không thể ẩn!",
      icon: "error",
      confirmButtonText: "OK",
    });
    loadData(true);
    return;
  }
  let data = {
    IntID: value.group_id,
    TextID: value.group_id + "",
    IntTrangthai: num,
    BitTrangthai: num == 0 ? value.status : value.is_default,
  };
  axios
    .put(
      baseURL + "/api/task_browser_group/Update_Default_Or_Status",
      data,
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
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
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const deleteGroup = (type, data) => {
  //type 0 : nhiều ; 1: 1;
  let f = [];
  if (type == 0) {
    f = selectedEmailGroups.value.filter((x) => x.is_default == true);
  }
  if (f.length > 0 || (type == 1 && data.is_default == true)) {
    swal.fire({
      title: "Thông báo!",
      text: "Không thể xóa nhóm mặc định!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let listId = new Array(type == 0 ? selectedEmailGroups.value.length : 1);
  swal
    .fire({
      title: "Thông báo",
      text:
        type == 0
          ? "Bạn có muốn xoá danh sách này không!"
          : "Bạn có muốn xóa nhóm duyệt này không ?",
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
        if (type == 0) {
          selectedEmailGroups.value.forEach((item) => {
            listId.push(item.group_id);
          });
        } else {
          listId.push(data.group_id);
        }
        axios
          .delete(baseURL + "/api/task_browser_group/Delete_group", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá nhóm duyệt thành công!");
              checkDelList.value = false;
              if (
                (options.value.totalRecords - listId.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
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
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  options.value.PageNo = event.page;

  loadData(true);
};
const basedomainURL = fileURL;
const delMember = (item, us) => {
  if (item != null) {
    swal
      .fire({
        title: "Thông báo",
        html: "Bạn có muốn xóa người duyệt này ?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có",
        cancelButtonText: "Không",
      })
      .then((result) => {
        if (result.isConfirmed) {
          var idx = us.user_info.findIndex((x) => x.user_id == item);
          if (idx != -1) {
            us.user_info.splice(idx, 1);
          }
          BrowserGroup.value = us;
          BrowserGroup.value.user_id = us.user_info;
          edit.value = true;

          saveData(true);
        }
      });
  }
};
const LoadHowToMark = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "Task_Marks_get",
            par: [{ par: "user_id", va: store.state.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      mark.value = data[0];
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "BrowseGroup.vue",
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
const mark = ref({});
const SwMark = () => {
  let data = {
    IntID: mark.value.mark_id,
    TextID: mark.value.mark_id + "",
    IntTrangthai: 1,
    BitTrangthai: mark.value.HowToCalcMark,
  };
  axios
    .put(baseURL + "/api/Task_MarkConfig/Update", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật cách tính điểm thành công!");
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
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const refresh = () => {
  loadData(true);
};
onMounted(() => {
  // for (let i = 1; i <= 20; i++) {
  //   BrowserGroup.value = {
  //     group_name: "1222",
  //     group_role: 1,
  //     is_default: false,
  //     status: true,
  //     organization_id: user.organization_id,
  //     user_id: [],
  //   };
  //   saveData(true);
  // }
  LoadHowToMark();
  loadData(true);
  return datalists;
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      v-model:first="first"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :value="datalists"
      :loading="options.loading"
      :paginator="true"
      :rows="options.PageSize"
      :totalRecords="options.totalRecords"
      dataKey="group_id"
      :rowHover="true"
      :showGridlines="true"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      v-model:selection="selectedEmailGroups"
      :class="'w-full'"
      @row-dblclick="showEmails($event.data)"
    >
      <template #header>
        <div class="col-12 p-0 flex">
          <div class="col-4 pl-0">
            <h3 class="module-title mt-0 ml-1 mb-2">
              <i class="pi pi-sliders-v"> </i> Tính điểm đánh giá
            </h3>
          </div>

          <div class="col-8 format-left">
            Tính điểm trung bình
            <InputSwitch
              v-model="mark.HowToCalcMark"
              class="ml-3 mr-3 myslide"
              @click="SwMark()"
            ></InputSwitch
            >Lấy điểm đánh giá cuối cùng
          </div>
        </div>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-at"> </i> Danh sách nhóm duyệt ({{
            options.totalRecords
          }})
        </h3>

        <Toolbar class="w-full custoolbar">
          <template #end>
            <Button
              v-if="checkDelList"
              @click="deleteGroup(0, null)"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="addNew('Thêm mới nhóm duyệt')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="refresh()"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />
          </template>
        </Toolbar>
      </template>

      <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:4rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:4rem; "
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="STT"
        header="STT"
        :sortable="false"
        headerStyle="text-align:center;max-width:4rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:4rem; "
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="group_name"
        header="Tên nhóm duyệt"
        :sortable="false"
        headerStyle="height:3.125rem"
        bodyStyle=" "
      >
      </Column>
      <Column
        field="group_role_name"
        header="Loại duyệt"
        headerStyle="text-align:center;max-width:12rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:12rem; "
        class="align-items-center justify-content-center text-center"
        ><template #body="data">
          {{ data.data.group_role_name }}
        </template>
      </Column>
      <Column
        field="user_count"
        header="Thành viên"
        headerStyle="text-align:center;max-width:150px;height:3.125rem"
        bodyStyle="text-align:center;max-width:150px; "
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div @click="showEmails(data.data)">
            <Chip
              :label="data.data.user_count ? data.data.user_count : '0'"
              class="px-3 text-xl bg-blue-300 text-black"
            >
            </Chip>
          </div>
        </template>
      </Column>
      <Column
        field="status"
        header="Hiển thị"
        headerStyle="text-align:center;max-width:8rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:8rem; "
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="data.data.status"
            v-model="data.data.status"
            @click="onCheckBox(data.data, 0)"
          />
        </template>
      </Column>
      <Column
        field="is_default"
        header="Mặc định"
        headerStyle="text-align:center;max-width:8rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:8rem; "
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="data.data.is_default"
            v-model="data.data.is_default"
            @click="onCheckBox(data.data, 1)"
          />
        </template>
      </Column>

      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:10rem;height:3.125rem;min-width:9.375rem;"
        bodyStyle="text-align:center;max-width:10rem ;min-width:9.375rem"
      >
        <template #body="data">
          <div>
            <Button
              @click="editOld(data.data, 'Sửa thông tin')"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="deleteGroup(1, data.data)"
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
          v-if="!isFirst"
        >
          <img
            src="../../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>
  <Sidebar
    v-model:visible="isShowEmail"
    position="right"
    :style="width < 1900 ? 'width: 45vw;' : 'width: 35vw;'"
    :showCloseIcon="false"
    @hide="hideall()"
  >
    <div class="overflow-hidden h-full w-full col-md-12 p-0 m-0">
      <div
        class="w-full h-9rem bg-var"
        style="--bg: #f8f9fa"
      >
        <h2 class="w-full h-full format-center text-blue-600">
          Danh sách người dùng ({{ EmitData.user_count }}) - Nhóm duyệt:
          {{ EmitData.group_name }}
        </h2>
      </div>
      <ScrollPanel
        :style="'height: calc(100vh - 9rem) !important'"
        v-if="EmitData.user_count > 0"
      >
        <div
          class="row col-12 flex p-0 m-0 font-bold my-div"
          v-for="(m, index) in EmitData.user_info"
          :key="m"
          style="border-bottom: 1px solid"
        >
          <div class="col-2 format-center p-0 m-0">{{ index + 1 }}</div>
          <div class="col-2 format-center p-0 m-0">
            <Avatar
              @error="
                $event.target.src = basedomainURL + '/Portals/Image/nouser1.png'
              "
              v-bind:label="
                m.avatar ? '' : m.full_name.split(' ').at(-1).substring(0, 1)
              "
              v-bind:image="basedomainURL + m.avatar"
              style="color: #ffffff; cursor: pointer"
              :style="{
                background: bgColor[index % 7],
                border: '2px solid' + bgColor[index % 7],
              }"
              class="col-2 p-0 m-0"
              size="large"
              shape="circle"
            />
          </div>
          <div class="col-7">
            <div class="col-12 pt-0">{{ m.full_name }}</div>
            <div class="col-12 pt-0">{{ m.position_name }}</div>
            <div class="col-12 pt-0">
              {{ m.department_name ? m.department_name : m.organization_name }}
            </div>
          </div>
          <div class="col-1 format-center">
            <!-- <Button
              icon="pi pi-trash"
              class="p-button-text p-button-danger mydelete"
              @click="delMember(m.user_id, EmitData)"
              v-tooltip="{ value: 'Xóa người duyệt' }"
            /> -->
          </div>
        </div>
      </ScrollPanel>
      <div
        class="align-items-center justify-content-center p-4 text-center m-auto"
        v-else
      >
        <img
          src="../../../assets/background/nodata.png"
          height="144"
        />
        <h3 class="m-1">Không có dữ liệu</h3>
      </div>
    </div>
  </Sidebar>
  <Dialog
    :header="headerDialog"
    v-model:visible="DialogVisible"
    :style="{ width: '40vw' }"
    :closable="true"
    :maximizable="true"
  >
    <div class="col-12 flex">
      <div class="col-4 format-left">
        Tên nhóm duyệt <span class="redsao pl-1">(*)</span>:
      </div>
      <InputText
        id="group_name"
        v-model="BrowserGroup.group_name"
        spellcheck="false"
        class="col-8"
        mode="decimal"
        showButtons
        :class="{
          'p-invalid': v$.group_name.$invalid && submitted,
        }"
        @input="checkconflix()"
        autocomplete="off"
      />
    </div>
    <div
      style="display: flex"
      class="col-12 p-0 pt-1"
      v-if="textboxLength > 250"
    >
      <div class="col-4 text-left"></div>
      <small class="col-8 p-0 pb-1 p-error">
        <span class="col-12">Tên nhóm duyệt không quá 250 ký tự!</span>
      </small>
    </div>
    <div
      style="display: flex"
      class="col-12 p-0 p-0"
      v-if="
        (v$.group_name.$invalid && submitted) ||
        v$.group_name.$pending.$response
      "
    >
      <div class="col-4 text-left"></div>
      <small class="col-8 p-0 pt-1 pb-1 p-error">
        <span class="col-12">{{
          v$.group_name.required.$message
            .replace("Value", "Tên nhóm duyệt")
            .replace("is required", "không được để trống")
        }}</span>
      </small>
    </div>

    <div class="col-12 flex">
      <div class="col-4 format-left">Kiểu duyệt</div>
      <Dropdown
        :filter="true"
        v-model="BrowserGroup.group_role"
        selectionLimit="1"
        :options="group_role"
        optionLabel="label"
        optionValue="code"
        spellcheck="false"
        class="col-8 p-0"
      >
        <template #option="slotProps">
          <div class="format-left">
            {{ slotProps.option.label
            }}<i
              class="pl-2 pi pi-info-circle"
              v-tooltip="{ value: slotProps.option.tip }"
            />
          </div>
        </template>
      </Dropdown>
    </div>
    <div class="col-12 flex">
      <div class="col-4 format-left">
        Người duyệt
        <Button
          icon="pi pi-user-plus text-xl"
          class="p-button-text ml-2 p-button-info ip36"
          v-tooltip.top="'Chọn người dùng'"
          @click="selectUser(false, 0)"
        ></Button>
      </div>
      <div
        style="display: flex"
        class="col-8 format-left"
        v-if="
          (v$.user_id.$invalid && submitted) || v$.user_id.$pending.$response
        "
      >
        <small class="col-12 p-0 p-error">
          <span class="col-12 p-0">{{
            v$.user_id.required.$message
              .replace("Value", "Danh sách người duyệt")
              .replace("is required", "không được để trống")
          }}</span>
        </small>
      </div>
    </div>

    <OrderList
      v-model="BrowserGroup.user_id"
      listStyle="height:auto"
      dataKey="user_id"
      class="col-12 p-0"
    >
      <template #header> Danh sách </template>
      <template #item="slotProps">
        <div class="flex">
          <div class="format-flex-center">
            <b class="p-3">{{ slotProps.index + 1 }}</b>
          </div>
          <div class="image-container pl-3 pr-2">
            <Avatar
              @error="
                $event.target.src = basedomainURL + '/Portals/Image/nouser1.png'
              "
              v-bind:label="
                slotProps.item.avatar
                  ? ''
                  : slotProps.item.full_name.split(' ').at(-1).substring(0, 1)
              "
              v-bind:image="basedomainURL + slotProps.item.avatar"
              :key="slotProps.item.user_id"
              style="border: 2px solid white; color: white"
              @error="basedomainURL + '/Portals/Image/noimg.jpg'"
              size="large"
              shape="circle"
              class="cursor-pointer bg-blue-200"
            />
          </div>
          <div
            class="format-grid-center justify-content-start"
            style="flex: 1"
          >
            <span class="text-left">{{ slotProps.item.full_name }}</span>
            <span
              class="text-left"
              style="color: #aaa"
              >{{ slotProps.item.position_name }}</span
            >
          </div>
          <div class="format-flex-center">
            <a class="btn-c-hover">
              <Button
                icon="pi pi-trash"
                class="p-button-text"
                @click="removeUser(slotProps.item, BrowserGroup.user_id)"
                v-tooltip.top="'Xóa người duyệt'"
              ></Button>
            </a>
          </div>
        </div>
      </template>
    </OrderList>
    <div class="col-12 flex">
      <div class="col-4 p-0 flex">
        <div class="col-3 p-0 format-left">STT</div>
        <InputNumber
          class="col-9"
          v-model="BrowserGroup.is_order"
          :useGrouping="false"
        ></InputNumber>
      </div>
      <div class="col-4 flex p-0">
        <div class="col-9 format-center">Nhóm mặc định</div>
        <div class="col-3 format-center">
          <InputSwitch v-model="BrowserGroup.is_default"></InputSwitch>
        </div>
      </div>
      <div class="col-4 flex p-0">
        <div class="col-8 format-center">Kích hoạt</div>
        <div class="col-4 format-center">
          <InputSwitch v-model="BrowserGroup.status"></InputSwitch>
        </div>
      </div>
    </div>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="DialogVisible = false"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveData(!v$.$invalid)"
      />
    </template>
  </Dialog>
  <treeuser
    :key="componentKey"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="is_one"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
</template>
<style scoped>
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-right {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-left {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: left;
}
.format-flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.format-grid-center {
  display: grid;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.btn-c-hover:hover {
  color: #0025f8 !important;
  background-color: white !important;
}
.bg-var {
  background-color: var(--bg);
}
.my-div:hover {
  background-color: #79bff8;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-inputswitch) {
  &.myslide {
    .p-inputswitch-slider {
      background-color: #2196f3;
    }
  }
}
.mydelete:hover {
  background-color: #ffffff !important;
}
</style>
