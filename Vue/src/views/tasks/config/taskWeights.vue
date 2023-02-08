<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../util/function.js";
import moment from "moment";
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const rules = {
  weight: {
    required,
  },
  weight_name: {
    required,
  },
  progress: { required },
};
const weights = ref({
  weight: null,
  weight_name: null,
  progress: null,
  status: true,
  organization_id: null,
});
const selectedDispatchs = ref([]);
const submitted = ref(false);
const v$ = useVuelidate(rules, weights);
const issaveDispatch = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
const options = ref({
  IsNext: true,
  sort: "weight",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_weights_count",
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
        sttDispatch.value = options.value.totalRecords + 1;
      }
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
const loadData = (rf) => {
  loadDonvi();
  if (rf) {
    if (isDynamicSQL.value) {
      loadDataSQL();
      return;
    }
    if (rf) {
      loadCount();
    }
    axios
      .post(
        baseURL + "/api/TaskProc/getTaskData",
        {
          str: encr(
            JSON.stringify({
              proc: "task_weights_list",
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
        if (isFirst.value) isFirst.value = false;
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
          element.tooltip =
            "Người tạo" +
            "<br/>" +
            element.full_name +
            "<br/>" +
            element.positions +
            "<br/>" +
            (element.department_name != null
              ? element.department_name
              : element.organiztion_name);
        });
        listweight.value = [];
        datalists.value = data;
        datalists.value.forEach((x) => {
          listweight.value.push(x.weight);
        });
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        addLog({
          title: "Lỗi Console loadData",
          controller: "SignerView.vue",
          logcontent: error.message,
          loai: 2,
        });
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
//Phân trang dữ liệu
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.PageNo + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.IsNext = false;
  } else if (event.page > options.value.PageNo) {
    //Trang sau

    options.value.id = datalists.value[datalists.value.length - 1].signer_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].signer_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};
let user = store.state.user;
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  select.value = {};
  checkWeights.value = false;
  submitted.value = false;
  weights.value = {
    weight_name: "",
    weight: null,
    status: true,
    progress: 0,
  };
  weights.value.organization_id = user.organization_id;
  issaveDispatch.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  weights.value = {
    weight: null,
    weight_name: "",
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};
const select = ref();
const checkWeights = ref(false);
//Thêm bản ghi
const listweight = ref();
const saveDispatch = (isFormValid) => {
  //checkWeights.value = datalists.value.includes(weights.value.weight);
  if (issaveDispatch.value != true) {
    checkWeights.value = listweight.value.includes(weights.value.weight);
    if (checkWeights.value == true) {
      return;
    }
  }
  if (select.value) {
    let id = parseInt(Object.keys(select.value)[0]);
    weights.value.department_id = id;
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
  axios({
    method: issaveDispatch.value ? "put" : "post",
    url:
      baseURL +
      `/api/taskWeights/${
        issaveDispatch.value ? "Update_Weights" : "add_Weights"
      }`,
    data: weights.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success(
          response.data.method == "put"
            ? "Cập nhật trọng số thành công!"
            : "Thêm trọng số thành công",
        );
        loadData(true);
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html: ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        loadData(true);
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

const sttDispatch = ref();
//Thêm bản ghi con
const isChirlden = ref(false);

//Sửa bản ghi
const editDispatch = (dataPlace) => {
  select.value = {};
  checkWeights.value = false;

  submitted.value = false;
  weights.value = dataPlace;
  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
  }
  headerDialog.value = "Sửa  trọng số";
  issaveDispatch.value = true;
  displayBasic.value = true;
  weights.value.organization_id = store.state.user.organization_id;
  if (weights.value.department_id) {
    select.value[dataPlace.department_id || "-1"] = true;
  }
};
//Xóa bản ghi
const delDispatch = (Dispatch) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá trọng số này không!",
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
          .delete(baseURL + "/api/taskWeights/Delete_Weights", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Dispatch != null ? [Dispatch.weight_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá trọng số thành công!");
              if (
                (options.value.totalRecords - Dispatch.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo",
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
                title: "Thông báo",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

//Xuất excel

//Sort
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    options.value.sort = "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  options.value.PageNo = 0;
  isDynamicSQL.value = true;
  loadDataSQL();
};
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  filterPhanloai.value =
    filterPhanloai.value == undefined ? null : filterPhanloai.value;
  let fpl;
  if (filterPhanloai.value != undefined && store.state.user.is_super) {
    fpl = parseInt(Object.keys(filterPhanloai.value)[0]);
  }
  let data = {
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    sqlF: store.state.user.is_super ? fpl : store.state.user.organization_id,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_Signer", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
          if (options.value.sort == "is_order DESC") {
            element.STT =
              options.value.totalRecords -
              options.value.PageNo * options.value.PageSize -
              i;
            if (options.value.sort == "is_order DESC") {
              {
                element.STT =
                  options.value.totalRecords -
                  options.value.PageNo * options.value.PageSize -
                  i;
              }
            }
          }
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
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
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
};
//Tìm kiếm

const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key != "signer_name" ? "signer_name" : key,
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
  options.value.PageNo = 1;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.weight_id,
    TextID: value.weight_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  axios
    .put(baseURL + "/api/taskWeights/Update_status_weights", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
        loadData(true);
        closeDialog();
      } else {
        swal.fire({
          title: "Thông báo",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedDispatchs.value.length);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá danh sách này không!",
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
        selectedDispatchs.value.forEach((item) => {
          listId.push(item.weight_id);
        });
        axios
          .delete(baseURL + "/api/taskWeights/Delete_Weights", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách thành công!");
              checkDelList.value = false;
              if (
                (options.value.totalRecords - listId.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
              selectedDispatchs.value = [];
            } else {
              swal.fire({
                title: "Thông báo",
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
                title: "Thông báo",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const searchDispatch = () => {
  options.value.PageNo = 0;
  filterTrangthai.value = null;
  filterPhanloai.value = null;
  options.value.SearchText = "";
  isDynamicSQL.value = false;
  styleObj.value = "";
  options.value.loading = true;
  first.value = 0;
  options.value = {
    PageNo: 0,
    PageSize: 20,
  };
  selectedDispatchs.value = [];
  loadData(true);
};
const first = ref(0);
//Filter

const filterPhanloai = ref();
const filterTrangthai = ref();

watch(selectedDispatchs, () => {
  if (selectedDispatchs.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});

const styleObj = ref();

const loadDonvi = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_org_list",
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      treedonvis.value = [];
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        if (data.length > 0) {
          data.forEach((x) => {
            x = { key: x.organization_id, data: x, label: x.organization_name };
            treedonvis.value.push(x);
          });
        } else {
          treedonvis.value = [];
        }
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
};
const treedonvis = ref();
const loadDonvi1 = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_all_for_select",
            par: [
              {
                par: "is_super",
                va: store.getters.user.is_super == 1 ? true : false,
              },
              { par: "user_id", va: store.getters.user.user_id },
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

      if (data.length > 0) {
        let obj = renderTree(
          data[0],
          "organization_id",
          "organization_name",
          "phòng ban",
        );

        donvis.value = obj.arrtreeChils;
      } else {
        donvis.value = [];
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
};
const renderTree = (data, id, name, title) => {
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
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const donvis = ref();
const Imp = ref(false);

let files = [];
const removeFile = (event) => {
  files = [];
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
const Upload = () => {
  let checkFile;
  Imp.value = false;
  let formData = new FormData();
  if (files.length == 0) {
    checkFile = "Chưa có tệp tải lên!";
  }
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);

    if (
      file.name.includes(".xls") == true ||
      file.name.includes(".xlsx") == true ||
      file.name.includes(".xlsm") == true ||
      file.name.includes(".csv")
    ) {
      checkFile = null;
    } else {
      checkFile = "File không đúng định dạng! Vui lòng kiểm tra lại!";
    }
  }
  if (checkFile == null) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    axios
      .post(baseURL + "/api/ImportExcel/Import_Signer", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Nhập dữ liệu thành công");
          isDynamicSQL.value = false;
          loadData(true);
        } else {
          swal.close();
          swal.fire({
            title: "Thông báo",
            html: "Vui lòng kiểm tra lại: <br>" + response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    swal.close();
    swal.fire({
      title: "Thông báo",
      text: checkFile,
      icon: "error",
      confirmButtonText: "OK",
    });
  }
};
const item = "/Portals/Mau Excel/Mẫu Excel  trọng số.xlsx";
const checkconflix = () => {
  const textbox = document.getElementById("textbox");
  if (issaveDispatch.value != true) {
    checkWeights.value = listweight.value.includes(textbox.value);
  }
  return checkWeights.value;
};
const length = ref(false);
const checklenght = () => {
  length.value = false;
  const textbox = document.getElementById("textarea");
  if (textbox.value.length > 250) {
    length.value = true;
  }
  return length.value;
};
onMounted(() => {
  loadData(true);
  loadDonvi1();
  loadDonvi();
  return {};
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      v-model:first="first"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      v-model:selection="selectedDispatchs"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :totalRecords="options.totalRecords"
      dataKey="weight_id"
      :rowHover="true"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
      :globalFilterFields="['']"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-sliders-v"></i> Danh sách trọng số ({{
            options.totalRecords
          }})
        </h3>

        <Toolbar class="w-full custoolbar">
          <template #end>
            <Button
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="openBasic('Thêm  trọng số')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="searchDispatch"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />

            <!-- <Button
              label="Tiện ích"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            /> -->
            <Menu
              id="overlay_Export"
              ref="menuButs"
              :model="itemButs"
              :popup="true"
            />
          </template>
        </Toolbar>
      </template>

      <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px "
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="STT"
        header="STT"
        :sortable="false"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px; "
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="weight"
        header="Trọng số"
        :sortable="false"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem; "
        class="align-items-center justify-content-center text-center"
      >
      </Column>

      <Column
        field="weight_name"
        header="Tên trọng số"
        :sortable="false"
        headerStyle="height:50px"
        bodyStyle=""
      >
      </Column>
      <Column
        field="progress"
        header="Phần trăm công việc"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:15rem;height:50px"
        bodyStyle="text-align:center;max-width:15rem; "
      >
        <template #body="t">{{ t.data.progress }} % </template>
      </Column>
      <Column
        field="created_date"
        header="Ngày tạo"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem; "
      >
        <template #body="t">
          {{ moment(new Date(t.data.created_date)).format("DD/MM/YYYY") }}
        </template>
      </Column>
      <Column
        field="created_by"
        header="Người tạo"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem; "
      >
        <template #body="t">
          {{ t.data.full_name }}
          <!-- <Avatar
            v-tooltip.right="{
              value: t.data.tooltip,
              escape: true,
            }"
            v-bind:label="
              t.data.avt
                ? ''
                : t.data.full_name.split(' ').at(-1).substring(0, 1)
            "
            v-bind:image="basedomainURL + t.data.avt"
            style="color: #ffffff; cursor: pointer"
            :style="{
              background: '#fffa8d',
              border: '2px solid #fffa8d',
            }"
            class="flex p-0 m-0"
            size="normal"
            shape="circle"
          /> -->
        </template>
      </Column>

      <Column
        field="status"
        header="Hiển thị"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px; "
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="data.data.status"
            v-model="data.data.status"
            @click="onCheckBox(data.data)"
          />
        </template>
      </Column>

      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px; "
      >
        <template #body="data">
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == data.data.created_by ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id == data.data.organization_id)
            "
          >
            <Button
              @click="editDispatch(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="delDispatch(data.data, true)"
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

  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="false"
  >
    <form>
      <div style="display: flex; flex-wrap: wrap">
        <div class="col-12 p-0">
          <div class="col-12 px-0 format-center">
            <div class="col-3 text-left">
              Trọng số
              <i
                class="px-1 pi pi-info-circle"
                v-tooltip.bottom="{
                  value: 'Trọng số từ 0-100',
                }"
              ></i
              ><span class="pl-1 redsao">(*)</span>
            </div>
            <InputNumber
              id="textbox"
              v-model="weights.weight"
              spellcheck="false"
              class="col-9 p-0"
              mode="decimal"
              showButtons
              :disabled="issaveDispatch == true ? true : fasle"
              :min="0"
              :max="100"
              :useGrouping="false"
              :class="{
                'p-invalid': v$.weight.$invalid && submitted,
              }"
              @change="checkconflix()"
              autocomplete="off"
            />
          </div>
          <div
            style="display: flex"
            class="col-12 p-0 pt-1"
            v-if="checkWeights == true"
          >
            <div class="col-3 text-left"></div>
            <small class="col-9 p-0 p-error">
              <span class="col-12 p-0">Trọng số đã tồn tại</span>
            </small>
          </div>
          <div
            style="display: flex"
            class="col-12 p-0 p-0"
            v-if="
              (v$.weight.$invalid && submitted) || v$.weight.$pending.$response
            "
          >
            <div class="col-3 text-left"></div>
            <small class="col-9 p-0 pt-1 p-error">
              <span class="col-12">{{
                v$.weight.required.$message
                  .replace("Value", "Trọng số")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>
        <div class="col-12 p-0">
          <div class="col-12 px-0 format-center">
            <div class="col-3 text-left">
              Tên trọng số <span class="redsao">(*)</span>
            </div>
            <Textarea
              id="textarea"
              v-model="weights.weight_name"
              spellcheck="false"
              class="col-9 ip36"
              :class="{
                'p-invalid': v$.weight_name.$invalid && submitted,
              }"
              @input="checklenght()"
            />
          </div>
          <div
            style="display: flex"
            class="col-12 p-0 pt-1"
            v-if="length == true"
          >
            <div class="col-3 text-left"></div>
            <small class="col-9 p-0 p-error">
              <span class="col-12 p-0">Tên trọng số không quá 250 kí tự</span>
            </small>
          </div>
          <div
            style="display: flex"
            class="col-12 p-0 p-0"
            v-if="
              (v$.weight_name.$invalid && submitted) ||
              v$.weight_name.$pending.$response
            "
          >
            <div class="col-3 text-left"></div>
            <small class="col-9 p-0 pt-1 p-error">
              <span class="col-12">{{
                v$.weight_name.required.$message
                  .replace("Value", "Tên trọng số")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>

        <div class="col-12 p-0 flex">
          <div class="col-6">
            <div class="col-12 p-0 flex">
              <div class="col-6 pl-0 format-left">Phần trăm công việc</div>
              <div class="col-6 pl-0">
                <InputNumber
                  inputId="minmax-buttons"
                  mode="decimal"
                  showButtons
                  :min="0"
                  :max="100"
                  suffix=" %"
                  class="w-full ip36 format-center"
                  v-tooltip.top="{
                    value: 'Phần trăm công việc <br/> ( 0 <= x <= 100 )',
                  }"
                  v-model="weights.progress"
                />
              </div>
            </div>
          </div>
          <div class="col-6">
            <div class="col-12 flex">
              <div class="col-6 format-center">Hiển thị</div>
              <div class="col-6">
                <InputSwitch
                  class="p-0 ip36"
                  v-model="weights.status"
                />
              </div>
            </div>
          </div>
        </div>

        <div
          style="display: flex"
          class="col-12 p-0"
          v-if="
            (v$.progress.$invalid && submitted) ||
            v$.progress.$pending.$response
          "
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-0 pt-1 p-error">
            <span class="col-12">{{
              v$.progress.required.$message
                .replace("Value", "Phần trăm công việc")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveDispatch(!v$.$invalid)"
      />
    </template>
  </Dialog>
  <Dialog
    header="Tải lên file Excel"
    v-model:visible="Imp"
    :style="{ width: '40vw' }"
    :closable="true"
  >
    <h3>
      <label>
        <a
          :href="basedomainURL + item"
          download
          >Nhấn vào đây</a
        >
        để tải xuống tệp mẫu.
      </label>
    </h3>
    <form>
      <FileUpload
        accept=".xls,.xlsx"
        @remove="removeFile"
        @select="selectFile"
        :show-upload-button="false"
        choose-label="Chọn tệp"
        cancel-label="Hủy"
        :fileLimit="1"
        :invalidFileTypeMessage="'Chỉ chấp nhận file dạng .xsl,.xlsx,.slsm,.csv'"
      >
        <template #empty>
          <p>Kéo và thả tệp vào đây để tải lên.</p>
        </template>
      </FileUpload>
    </form>
    <template #footer>
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="Upload"
      />
    </template>
    <!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
  </Dialog>
</template>

<style scoped>
.p-treeselect-panel {
  max-width: 30vw !important;
}
.p-treeselect-panel .p-treeselect-items-wrapper .p-tree {
  max-height: 17vh !important;
}
.p-dropdown-item {
  white-space: normal !important;
}
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
  text-align: center;
}
</style>
