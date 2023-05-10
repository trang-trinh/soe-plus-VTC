<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { required, maxLength, minLength, email } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import DropdownProfile from "../component/DropdownProfile.vue";
import VueBlocksTree from "vue3-blocks-tree";
import "vue3-blocks-tree/dist/vue3-blocks-tree.css";
// import router from "@/router";
import moment from "moment";
import { encr, autoFillDate } from "../../../util/function.js";
const cryoptojs = inject("cryptojs");

const emitter = inject("emitter");
const getProfileUser=(user,obj)=>{
 
  if (obj) {
    department_merger.value[user]=obj.profile_id;
      } else {
        department_merger.value[user] = null;
      }
}
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "submitDropdownUser":
      if (obj.data) {
        department_merger.value.signer = obj.data.profile_id;
      } else {
        department_merger.value.signer = null;
      }

      break;

    default:
      break;
  }
});
//init Model
const tdorganization_types = [
  { value: 0, text: "Đơn vị" },
  // { value: 1, text: "Trường học" },
  { value: 1, text: "Phòng ban" },
];
const layout = ref("list");
const donvi = ref({
  organization_name: "",
  is_order: 1,
  status: true,
  organization_type: 0,
});
//color
const opColor = ref();
let pcolor = "";
const toggleColor = (event, pc) => {
  opColor.value.toggle(event);
  pcolor = pc;
};
const changeColor = (color) => {
  donvi.value[pcolor] = color.hex;
  if (!color.hex.includes("#")) opColor.value.hide();
};
//Valid Form
const submitted = ref(false);
const rules = {
  organization_name: {
    required,
    maxLength: maxLength(500),
  },
  mail: {
    email,
  },
};
const v$ = useVuelidate(rules, donvi);
//Khai báo biến
const expandedKeys = ref({});

const isDisplayAvt = ref(false);
const isDisplayNen = ref(false);
const store = inject("store");
const selectCapcha = ref();
const selectedKey = ref();
const selectedNodes = ref({});
const filters = ref({});

const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 10000,
  organization_type: null,
  user_id: store.getters.user.user_id,
});
const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  loadingP: true,
  pagenoP: 0,
  pagesizeP: 20,
  searchP: "",
  sortP: "created_date",
});
const donvis = ref();
const data_org = ref();
const treedonvis = ref();
const orgchartDonvi = ref({});
const checkShowOrgChart = ref(false);
const selectDiadanh = ref();
const displayAddDonvi = ref(false);
const isFirst = ref(true);
let files = {};
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const onNodeSelect = (node) => {
  selectedNodes.value = node.data.organization_id;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  axios
    .post(
      baseURL + "/api/Phongban/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_get",
            par: [{ par: "organization_id", va: node.data.organization_id }],
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
        donvi.value = data[0][0];
        if (donvi.value.foundation_date)
          donvi.value.foundation_date = new Date(donvi.value.foundation_date);
        if (donvi.value.dissolution_date)
          donvi.value.dissolution_date = new Date(donvi.value.dissolution_date);
        selectCapcha.value = {};
        selectCapcha.value[donvi.value.parent_id || "-1"] = true;
        selectDiadanh.value = {};
        selectDiadanh.value[donvi.value.place_id || "-1"] = true;
      }

      initReward(node.data.organization_id);
      visibleRight.value = true;
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
const onNodeUnselect = (node) => {
  selectedNodes.value = null;
};
const handleFileUpload = (event, ia) => {
  if (ia == "LogoDonvi") isDisplayAvt.value = true;
  else if (ia == "LogoNen") isDisplayNen.value = true;
  files[ia] = event.target.files[0];
  var output = document.getElementById(ia);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
// on event
const onChangeParent = (item) => {
  const organization_id = parseInt(Object.keys(item)[0]);
  axios
    .post(
      baseURL + "/api/Phongban/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_get_order",
            par: [{ par: "organization_id", va: organization_id }],
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
        donvi.value.is_order = data[0][0].c + 1;
      }
    });
};
//Show Modal
const showModalAddDonvi = () => {
  files = [];
  submitted.value = false;
  selectCapcha.value = {};
  // selectCapcha.value[store.getters.user.parent_id] = true;
  donvi.value = {
    organization_name: "",
    is_order: donvis.value.length + 1,
    status: true,
    organization_type: 0,
    // parent_id: null,
    // parent_id: store.getters.user.organization_id,
  };
  displayAddDonvi.value = true;
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddDonvi = () => {
  displayAddDonvi.value = false;
};
//xóa ảnh
const delLogo = () => {
  files["LogoDonvi"] = [];
  isDisplayAvt.value = false;
  var output = document.getElementById("LogoDonvi");
  output.src = basedomainURL + "/Portals/Image/noimg.jpg";
  donvi.value.logo = null;
};
const delNen = () => {
  files["LogoNen"] = [];
  isDisplayNen.value = false;
  var output = document.getElementById("LogoNen");
  output.src = basedomainURL + "/Portals/Image/noimg.jpg";
  donvi.value.background_image = null;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  selectedKey.value = {};
  selectedNodes.value = null;
  loadDonvi(true);
};

const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      m.label_order = m.IsOrder.toString();
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, index) => {
            em.label_order = mm.data.label_order + "." + (index + 1);
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

const renderOrgChart = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  const rechildren = (mm, pid) => {
    let dts = data.filter((x) => x.parent_id == pid);

    if (dts.length > 0) {
      if (!mm.children) mm.children = [];
      dts.forEach((em, index) => {
        let om1 = {
          label: em.organization_name + "",
          expand: false,
          some_id: em[id],
          logo: em.logo,
          type: em.organization_type,
        };
        if (!store.getters.user.is_super) {
          om1.expand = true;
        }

        rechildren(om1, em[id]);
        mm.children.push(om1);
      });
    }
  };
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      let om = {
        label: m.organization_name + "",
        expand: true,
        some_id: m[id],
        logo: m.logo,
        type: m.organization_type,
      };

      rechildren(om, m[id]);
      arrChils.push(om);
    });

  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const listTreeSelect = ref({});
const department_merger = ref({});
const reloadLayout = () => {
  axios
    .post(
      baseURL + "/api/Phongban/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_main",
            par: [
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
              { par: "organization_type", va: opition.value.organization_type },
              { par: "user_id", va: opition.value.user_id },
            ],
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
        if (layout.value == "list") {
          let obj = renderTree(
            data[0],
            "organization_id",
            "organization_name",
            "đơn vị"
          );
          donvis.value = obj.arrChils;
          listTreeSelect.value = obj.arrtreeChils;
          data_org.value = data[0];
          if (!store.getters.user.is_super) {
            donvis.value.forEach((element) => {
              expandNode(element);
            });
          } else {
            expandedKeys.value[store.getters.user.organization_id] = true;
          }
          treedonvis.value = obj.arrtreeChils;
          opition.value.totalRecords = data[1][0].totalrecords;
        } else {
          let obj = renderOrgChart(
            data[0],
            "organization_id",
            "organization_name",
            "đơn vị"
          );
          orgchartDonvi.value = obj.arrChils[0];
          checkShowOrgChart.value = true;
        }
      } else {
        orgchartDonvi.value = [];
      }

      if (isFirst.value) isFirst.value = false;
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const onNodeSelectOrgchart = (event) => {
  document.getElementById("orgchart"+event.some_id).style.backgroundColor='#f0f8ff';
  selectedNodes.value = event.some_id;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  axios
    .post(
      baseURL + "/api/Phongban/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_get",
            par: [{ par: "organization_id", va: event.some_id }],
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
        donvi.value = data[0][0];
        if (donvi.value.foundation_date)
          donvi.value.foundation_date = new Date(donvi.value.foundation_date);
        if (donvi.value.dissolution_date)
          donvi.value.dissolution_date = new Date(donvi.value.dissolution_date);
        selectCapcha.value = {};
        selectCapcha.value[donvi.value.parent_id || "-1"] = true;
        selectDiadanh.value = {};
        selectDiadanh.value[donvi.value.place_id || "-1"] = true;
      }

      initReward(event.some_id);
      visibleRight.value = true;
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
const loadDonvi = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Phongban/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_main",
            par: [
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
              { par: "organization_type", va: opition.value.organization_type },
              { par: "user_id", va: opition.value.user_id },
            ],
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
        let obj = renderTree(
          data[0],
          "organization_id",
          "organization_name",
          "đơn vị"
        );
        donvis.value = obj.arrChils;
        listTreeSelect.value = obj.arrtreeChils;
        data_org.value = data[0];
        if (!store.getters.user.is_super) {
          donvis.value.forEach((element) => {
            expandNode(element);
          });
        } else {
          expandedKeys.value[store.getters.user.organization_id] = true;
        }
        treedonvis.value = obj.arrtreeChils;
        opition.value.totalRecords = data[1][0].totalrecords;
      } else {
        donvis.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opition.value.loading = false;
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    // for (let child of node.children) {
    //   expandNode(child);
    // }
  }
};
 
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (donvi.value.organization_name.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text:
        "Vui lòng không được nhập tên " +
        (donvi.value.organization_type == 1 ? "phòng ban" : "đơn vị") +
        " quá 500 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
  }
  if (selectCapcha.value != null) {
    let keys = Object.keys(selectCapcha.value);
    donvi.value.parent_id = keys[0];
    if (donvi.value.parent_id == -1) {
      donvi.value.parent_id = null;
    }
  }
  if (selectDiadanh.value) {
    let keys = Object.keys(selectDiadanh.value);
    donvi.value.place_id = keys[0];
    if (donvi.value.place_id == -1) {
      donvi.value.place_id = null;
    }
  }

  addDonvi();
};

const addDonvi = () => {
  let formData = new FormData();
  for (var k in files) {
    let file = files[k];
    formData.append(k, file);
  }
  formData.append("model", JSON.stringify(donvi.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: donvi.value.organization_id ? "put" : "post",
    url:
      baseURL +
      `/api/Phongban/${
        donvi.value.organization_id ? "Update_Donvi" : "Add_Donvi"
      }`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        donvi.value.organization_type == 0
          ? toast.success("Cập nhật đơn vị thành công!")
          : toast.success("Cập nhật phòng ban thành công!");
        if (layout.value == "list") loadDonvi();
        else initTreeDV();
        closedisplayAddDonvi();
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
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const delOrgHistory = (Tem) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bản ghi này không!",
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
              "/api/sys_organization_history/delete_sys_organization_history",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: Tem != null ? [Tem.organization_history_id] : 1,
            }
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bản ghi thành công!");
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
// xóa đơn vị

// xóa nhiều

const exportDonvi = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "Sys_Donvi" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH ĐƠN VỊ",
        proc: "Sys_Donvi_ListExport",
        par: par,
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
        if (response.data.path != null) {
          let pathReplace = response.data.path
            .replace(/\\+/g, "/")
            .replace(/\/+/g, "/")
            .replace(/^\//g, "");
          var listPath = pathReplace.split("/");
          var pathFile = "";
          listPath.forEach((item) => {
            if (item.trim() != "") {
              pathFile += "/" + item;
            }
          });
          //window.open(baseURL + response.data.path);
          window.open(baseURL + pathFile);
        }
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
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};

const displayStructure = ref(false);
const headerStructure = ref("Sáp nhập");
const checkStructure = ref(false);
const onShowStructure = () => {
  headerStructure.value = "Sáp nhập";
  checkStructure.value = true;
  displayStructure.value = true;
};
const onDissolution = () => {
  headerStructure.value = "Giải thể";
  checkStructure.value = false;
  displayStructure.value = true;
};
watch(layout, () => {
  if (layout.value == "grid") {
    initTreeDV();
  }
  if (layout.value == "list") {
    loadDonvi(true);
  }
});
//tree

const initTreeDV = (rf) => {
  axios
    .post(
      baseURL + "/api/Phongban/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_main_new",
            par: [
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va: options.value.PageSize },
              { par: "search", va: options.value.search },
              { par: "user_id", va: store.getters.user.user_id },
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
      if (isFirst.value) isFirst.value = false;
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        element.isClosed = false;
        element.isOpened = true;
        if (data.find((x) => x.parent_id == element.organization_id)) {
          element.canExpand = true;
        }
      });
      donvitrees.value = data;
      opition.value.loading = false;

      if (data[0] != null)
        loadDataDetails(data[0].organization_id, data[0].organization_name);
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const id_active = ref();
const department_name = ref();
const datalistsDetails = ref();
const loadDataDetails = (id, name) => {
  id_active.value = id;
  department_name.value = name;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "sys_user_listby_department",
        par: [
          { par: "organization_id", va: id },
          { par: "search", va: options.value.SearchText },
          { par: "pageno", va: options.value.pagenoP },
          { par: "pagesize", va: options.value.pagesizeP },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
      });
      if (isFirst.value) isFirst.value = false;
      let obj = renderTree(
        data,
        "organization_id",
        "organization_name",
        "phòng ban"
      );
      // treemodules.value = obj.arrtreeChils;

      datalistsDetails.value = obj.arrChils;
      options.value.loadingP = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");

      options.value.loading = false;
    });
};
const item_active = ref();
const donvitrees = ref();
const toggle_Donvisudung = (dv) => {
  item_active.value = dv.organization_id;
  dv.isOpened = !dv.isOpened;
  var lst = donvitrees.value.filter((l) => l.parent_id === dv.organization_id);
  lst.forEach((o) => {
    o.isClosed = !o.isClosed;
    if (o.isClosed) {
      Expanded(o);
    }
  });
};
function Expanded(dv) {
  dv.isOpened = false;
  var lst = donvitrees.value.filter((l) => l.parent_id === dv.organization_id);
  if (lst !== null || lst.length > 0) {
    lst.forEach((o) => {
      o.isClosed = true;
      if (o.isClosed) {
        Expanded(o);
      }
    });
  }
}

const org_history = ref({});

const visibleRight = ref(false);
const onHideSidebar = () => {
  if(layout.value=="grid")
  document.getElementById("orgchart"+donvi.value.organization_id).style.backgroundColor='unset';
  selectedKey.value = null;
  selectedNodes.value = null;
};
const closeSidebar = () => {
  visibleRight.value = false;
};

const listRewards = ref([]);
const initReward = (organization_id) => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_reward_list_organization",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "organization", va: organization_id },
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
      let data1 = JSON.parse(response.data.data)[1];

      if (isFirst.value) isFirst.value = false;

      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
      });
      listRewards.value = data;

      listOrgLog.value = data1;
      data1.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
      });
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};
const listOrgLog = ref([]);
const filesList = ref([]);
let fileSize = [];
const onUploadFile = (event) => {
  fileSize = [];
  filesList.value = [];

  var ms = false;

  event.files.forEach((fi) => {
    let formData = new FormData();
    formData.append("fileupload", fi);
    axios({
      method: "post",
      url: baseURL + `/api/chat/ScanFileUpload`,
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          if (fi.size > 100 * 1024 * 1024) {
            ms = true;
          } else {
            filesList.value.push(fi);
            fileSize.push(fi.size);
          }
        } else {
          filesList.value = filesList.value.filter((x) => x.name != fi.name);
          swal.fire({
            title: "Cảnh báo",
            text: "File bị xóa do tồn tại mối đe dọa với hệ thống!",
            icon: "warning",
            confirmButtonText: "OK",
          });
        }
        if (ms) {
          swal.fire({
            icon: "warning",
            type: "warning",
            title: "Thông báo",
            text: "Bạn chỉ được upload file có dung lượng tối đa 100MB!",
          });
        }
      })
      .catch(() => {
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
  filesList.value = filesList.value.filter((a) => a != event.file);
};
const closeStructure = () => {
  displayStructure.value = false;
};
const listFilesS = ref([]);
const saveOrganization = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (donvi.value.organization_name.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text:
        "Vui lòng không được nhập tên " +
        (donvi.value.organization_type == 1 ? "phòng ban" : "đơn vị") +
        " quá 500 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
  }
  if (selectCapcha.value != null) {
    let keys = Object.keys(selectCapcha.value);
    donvi.value.parent_id = keys[0];
    if (donvi.value.parent_id == -1) {
      donvi.value.parent_id = null;
    }
  }
  if (selectDiadanh.value) {
    let keys = Object.keys(selectDiadanh.value);
    donvi.value.place_id = keys[0];
    if (donvi.value.place_id == -1) {
      donvi.value.place_id = null;
    }
  }

  let formData = new FormData();
  formData.append("model", JSON.stringify(donvi.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + `/api/Phongban/Update_Donvi`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        donvi.value.organization_type == 0
          ? toast.success("Cập nhật đơn vị thành công!")
          : toast.success("Cập nhật phòng ban thành công!");
        if (layout.value == "list") loadDonvi();
        else initTreeDV();
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
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const propsOrgchartDonvi = ref({
  label: "label",
  expand: "expand",
  children: "children",
  key: "key",
});
const displayOrgHistory = ref(false);
const headerOrgHistory = ref("");

const onAddOrgHistory = (data) => {
   
  org_history.value = data;
  isView.value = false;

  isSaveOrgHistory.value = true;
  headerOrgHistory.value = "Thêm lịch sử thay đổi";
  displayOrgHistory.value = true;
};
const checkAddOrd = ref(false);
const isEditOrgHistory = () => {
  checkAddOrd.value = true;
  isSaveOrgHistory.value = false;
  isView.value = false;
};
const editOrgHistory = (data) => {
  axios
    .post(
      baseURL + "/api/HRM_SQL/GetData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_history_get",
            par: [
              {
                par: "organization_history_id",
                va: data.organization_history_id,
              },
            ],
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
        org_history.value = data[0][0];
        if (org_history.value.foundation_date)
          org_history.value.foundation_date = new Date(
            org_history.value.foundation_date
          );
        if (org_history.value.dissolution_date)
          org_history.value.dissolution_date = new Date(
            org_history.value.dissolution_date
          );
        if (org_history.value.decision_date)
          org_history.value.decision_date = new Date(
            org_history.value.decision_date
          );
        listFilesS.value = data[1];
        checkAddOrd.value = false;
        isView.value = true;
        isSaveOrgHistory.value = false;

        headerOrgHistory.value = "Chi tiết lịch sử thay đổi";
        displayOrgHistory.value = true;
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
const isView = ref(false);

const isSaveOrgHistory = ref(false);
const saveOrgHistory = () => {
  submitted.value = true;
  if (org_history.value.decision_number == null) {
    return;
  }

  let formData = new FormData();
  formData.append("model", JSON.stringify(org_history.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }

  formData.append(
    "sys_organization_history",
    JSON.stringify(org_history.value)
  );
  formData.append("hrm_files", JSON.stringify(listFilesS.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (isSaveOrgHistory.value == true) {
    axios
      .post(
        baseURL + "/api/sys_organization_history/add_sys_organization_history",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm lịch sử đơn vị thành công!");

          displayOrgHistory.value = false;
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
      .put(
        baseURL +
          "/api/sys_organization_history/update_sys_organization_history",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa lịch sử đơn vị thành công!");

          displayOrgHistory.value = false;
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
const closeOrgHistory = () => {
  org_history.value = {};
  displayOrgHistory.value = false;
};

onMounted(() => {
  //init
  loadDonvi(true);
  //loadTudien();
});
</script>
<template>
  <div class="surface-100">
    <div
      class="main-layout flex-grow-1 p-2"
      v-if="layout == 'list'"
      style="height: calc(100vh - 52px)"
    >
      <TreeTable
        :value="donvis"
        v-model:selectionKeys="selectedKey"
        :loading="opition.loading"
        @nodeSelect="onNodeSelect"
        @nodeUnselect="onNodeUnselect"
        :expandedKeys="expandedKeys"
        :filters="filters"
        :showGridlines="false"
        selectionMode="single"
        filterMode="strict"
        class="p-treetable-sm d-tree-missheader"
        :rows="20"
        :rowHover="true"
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        :metaKeySelection="false"
      >
        <template #header>
          <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
            <i class="pi pi-sitemap"></i>
            Cơ cấu tổ chức
          </h3>
          <Toolbar class="w-full custoolbar">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  type="text"
                  spellcheck="false"
                  v-model="filters['global']"
                  placeholder="Tìm kiếm"
                />
              </span>
            </template>

            <template #end>
              <Button
                icon="pi pi-link"
                label="Sáp nhập"
                class="font-bold w-9rem mr-2"
                @click="onShowStructure"
              />
              <Button
                @click="onDissolution"
                icon="pi pi-stop-circle"
                label="Giải thể"
                class="p-button-outlined font-bold p-button-danger w-9rem mr-2"
              />
              <DataViewLayoutOptions
                v-model="layout"
                @update:modelValue="reloadLayout"
              />

              <Button
                class="mr-2 ml-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="onRefersh"
              />
            </template>
          </Toolbar>
        </template>

        <Column field="organization_name" header="Tên đơn vị" :expander="true">
          <template #body="md">
            <span
              :class="'donvi' + md.node.data.organization_type"
              :style="[
                md.node.data.parent_id ? '' : 'font-weight:bold',
                md.node.data.status ? '' : 'color:red !important',
              ]"
              >{{ md.node.data.organization_name }}</span
            >
          </template>
        </Column>

        <!-- <Column
        header="Chức năng"
        headerClass="text-center"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-plus"
            class="p-button-rounded p-button-secondary p-button-outlined"
            style="margin-right: 0.5rem"
            v-tooltip.top="'Thêm đơn vị trực thuộc'"
            @click="addTreeDonvi(md.node.data, 1)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-pencil"
            v-tooltip.top="'Chỉnh sửa'"
            class="p-button-rounded p-button-secondary p-button-outlined"
            style="margin-right: 0.5rem"
            @click="editDonvi(md.node.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            v-tooltip.top="'Xóa'"
            class="p-button-rounded p-button-secondary p-button-outlined"
            @click="delDonvi(md.node.data)"
          ></Button>
        </template>
      </Column> -->
        <template #empty>
          <div
            class="m-auto align-items-center justify-content-center p-4 text-center"
            v-if="!isFirst"
          >
            <img src="../../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
      </TreeTable>
    </div>
    <div v-if="layout == 'grid'">
      <div class="w-full surface-0 m-2 pt-2 pl-2">
        <div class="surface-0">
          <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
            <i class="pi pi-sitemap"></i>
            Cơ cấu tổ chức
          </h3>
          <Toolbar class="w-full custoolbar">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  type="text"
                  spellcheck="false"
                  v-model="filters['global']"
                  placeholder="Tìm kiếm"
                />
              </span>
            </template>

            <template #end>
              <Button
                icon="pi pi-link"
                label="Sáp nhập"
                class="font-bold w-9rem mr-2"
                @click="onShowStructure"
              />
              <Button
                @click="onDissolution"
                icon="pi pi-stop-circle"
                label="Giải thể"
                class="p-button-outlined font-bold p-button-danger w-9rem mr-2"
              />
              <DataViewLayoutOptions
                v-model="layout"
                @update:modelValue="reloadLayout"
              />

              <Button
                class="mr-2 ml-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="onRefersh"
              />
            </template>
          </Toolbar>
        </div>
        <div
          style="overflow: scroll; width: 86vw; height: 85vh"
          v-if="checkShowOrgChart"
          class="align-items-center"
        >
          <VueBlocksTree
            :data="orgchartDonvi"
            :props="propsOrgchartDonvi"
            :horizontal="true"
            :collapsable="true"
            class="d-design-orgchart"
          >
            <template #node="{ data }">
              <div
                :id="'orgchart' + data.some_id"
                style="padding: 10px 15px"
                class="flex flex-column align-items-center cursor-pointer d-design-org-hover"
                @click="onNodeSelectOrgchart(data)"
              >
                <div
                  v-if="data.type == 0"
                  class="flex flex-column align-items-center"
                >
                  <div v-if="data.logo" style="width: 132px; height: 38px">
                    <img
                      :alt="data.label"
                      :src="basedomainURL + data.logo"
                      width="132"
                      height="38"
                      style="object-fit: contain"
                    />
                  </div>
                  <div
                    v-else
                    style="width: 132px; height: 38px; color: transparent"
                  >
                    aassaaa
                  </div>
                  <div class="mt-3 font-medium text-lg">
                    {{ data.label }}
                  </div>
                </div>
                <div v-else class="font-medium text-lg">
                  {{ data.label }}
                </div>
              </div>
            </template>
          </VueBlocksTree>

          <!-- @node-expand="(e, data,nodeContext) => $emit('node-expand', e, data,nodeContext)"
        @node-focus="(e, data,nodeContext) => $emit('node-focus', e, data,nodeContext)"
        @node-click="(e, data,nodeContext) => $emit('node-click', e, data,nodeContext)"
        @node-mouseover="(e, data,nodeContext) => $emit('node-mouseover', e, data,nodeContext)"
        @node-mouseout="(e, data,nodeContext) => $emit('node-mouseout', e, data,nodeContext)" -->
        </div>
      </div>
    </div>
  </div>
  <Sidebar
    v-model:visible="visibleRight"
    position="right"
    @hide="onHideSidebar"
    style="width: 66%"
    :showCloseIcon="false"
    class="d-design-sidebar"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <h3>
            1.
            {{
              donvi.organization_type == 1
                ? "Thông tin phòng ban "
                : "Thông tin đơn vị "
            }}
          </h3>
        </div>

        <div class="col-12 field flex align-items-center">
          <label class="w-10rem text-left"
            >Tên{{ donvi.organization_type == 1 ? " phòng ban " : " đơn vị "
            }}<span class="redsao">(*)</span></label
          >
          <InputText
            spellcheck="false"
            class="ip36"
            style="width: calc(100% - 10rem)"
            v-model="donvi.organization_name"
            :class="{
              'p-invalid': v$.organization_name.$invalid && submitted,
            }"
          />
        </div>

        <small
          v-if="
            (v$.organization_name.required.$invalid && submitted) ||
            v$.organization_name.required.$pending.$response
          "
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3" v-if="donvi.organization_type == 0">{{
              v$.organization_name.required.$message
                .replace("Value", "Tên đơn vị")
                .replace("is required", "không được để trống")
            }}</span>
            <span class="col-10 pl-3" v-if="donvi.organization_type == 1">{{
              v$.organization_name.required.$message
                .replace("Value", "Tên phòng ban")
                .replace("is required", "không được để trống")
            }}</span>
          </div>
        </small>
        <small
          v-if="v$.organization_name.maxLength.$invalid && submitted"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3" v-if="donvi.organization_type == 0"
              >{{
                v$.organization_name.maxLength.$message.replace(
                  "The maximum length allowed is",
                  "Tên đơn vị không được vượt quá"
                )
              }}
              ký tự</span
            >
            <span class="col-10 pl-3" v-if="donvi.organization_type == 1"
              >{{
                v$.organization_name.maxLength.$message.replace(
                  "The maximum length allowed is",
                  "Tên phòng ban không được vượt quá"
                )
              }}
              ký tự</span
            >
          </div>
        </small>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Tên tiếng Anh</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="donvi.organization_name_en"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem pl-3 text-left">Tên viết tắt</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="donvi.short_name"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Thời gian hoạt động</label>

            <Calendar
              style="width: calc(100% - 10rem)"
              id="icon"
              v-model="donvi.foundation_date"
              :showIcon="true"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left pl-3">Đến</label>

            <Calendar
              style="width: calc(100% - 10rem)"
              id="icon"
              v-model="donvi.dissolution_date"
              :showIcon="true"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Người đại diện</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="donvi.representative"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem pl-3 text-left">Mã doanh nghiệp</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="donvi.business_code"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Địa điểm làm việc</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="donvi.address"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem pl-3 text-left">Điện thoại</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="donvi.phone"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex align-items-center">
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem text-left">Địa điểm ĐKKD</div>
            <Textarea
              :autoResize="true"
              rows="1"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="donvi.address_registration"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem text-left pl-3">Website</div>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="donvi.is_url"
            />
          </div>
        </div>

        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Email</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="donvi.mail"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem pl-3 text-left">Fax</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="donvi.fax"
            />
          </div>
        </div>
        <div class="col-12 flex align-items-center">
          <div class="w-10rem text-left">Chức năng nhiệm vụ</div>
          <Textarea
            :autoResize="true"
            rows="1"
            class="ip36"
            style="width: calc(100% - 10rem)"
            v-model="donvi.feature"
          />
        </div>

        <div class="col-12 md:col-12">
          <h3>2. Thông tin khen thưởng</h3>
        </div>
        <div class="col-12 md:col-12 field">
          <div class="d-lang-table">
            <DataTable
              filterDisplay="menu"
              filterMode="lenient"
              :scrollable="true"
              scrollHeight="flex"
              :showGridlines="true"
              columnResizeMode="fit"
              :reorderableColumns="true"
              :value="listRewards"
              dataKey="reward_id"
              responsiveLayout="scroll"
              :row-hover="true"
            >
              <Column
                field="STT"
                header="STT"
                class="align-items-center justify-content-center text-center overflow-hidden"
                headerStyle="text-align:center;max-width:55px;height:50px"
                bodyStyle="text-align:center;max-width:55px"
              >
              </Column>
              <Column
                field="reward_number"
                header="Số quyết định"
                headerStyle="text-align:center;max-width:120px;height:50px"
                bodyStyle="text-align:center;max-width:120px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
              </Column>

              <Column
                field="reward_content"
                header="Nội dung"
                headerStyle="text-align:left;height:50px"
                headerClass="align-items-center justify-content-center text-center overflow-hidden"
                bodyStyle="text-align:left"
              >
              </Column>
              <Column
                field="reward_level_name"
                header="Cấp khen thưởng "
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
              </Column>
              <Column
                field="reward_title_name"
                header="Hình thức"
                headerStyle="text-align:center;max-width:200px;height:50px"
                bodyStyle="text-align:center;max-width:200px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
              </Column>
              <Column
                field="start_date"
                header="Ngày quyết định"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
                <template #body="data">
                  <div v-if="data.data.decision_date">
                    {{
                      moment(new Date(data.data.decision_date)).format(
                        "DD/MM/YYYY"
                      )
                    }}
                  </div>
                </template>
              </Column>
              <Column
                header=" "
                headerStyle="text-align:center;max-width:50px;height:50px"
                bodyStyle="text-align:center;max-width:50px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
                <template #body="data">
                  <div v-if="data.data.file_path">
                    <a download :href="basedomainURL + data.data.file_path">
                      <Button
                        :v-tooltip.top="'Tệp đính kèm'"
                        icon="pi pi-paperclip"
                        class="p-button-rounded p-button-outlined"
                      />
                    </a>
                  </div>
                </template>
              </Column>
            </DataTable>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0 field flex align-items-center">
          <div class="col-6 md:col-6">
            <h3>3. Lịch sử thay đổi</h3>
          </div>
          <div
            class="col-6 md:col-6 text-right"
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == donvi.created_by ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id == donvi.organization_id)
            "
          >
            <Button
              label="Thêm mới lịch sử"
              icon="pi pi-plus"
              @click="onAddOrgHistory(donvi)"
            />
          </div>
        </div>
        <div class="col-12 field md:col-12">
          <div class="d-lang-table">
            <DataTable
              filterDisplay="menu"
              filterMode="lenient"
              :scrollable="true"
              scrollHeight="flex"
              :showGridlines="true"
              columnResizeMode="fit"
              :reorderableColumns="true"
              :value="listOrgLog"
              dataKey="organization_log_id"
              responsiveLayout="scroll"
              :row-hover="true"
            >
              <Column
                field="STT"
                header="STT"
                class="align-items-center justify-content-center text-center overflow-hidden"
                headerStyle="text-align:center;max-width:55px;height:50px"
                bodyStyle="text-align:center;max-width:55px"
              >
              </Column>
              <Column
                field="decision_number"
                header="Số quyết định"
                headerStyle="text-align:center;max-width:120px;height:50px"
                bodyStyle="text-align:center;max-width:120px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
              </Column>
              <Column
                field="content"
                header="Nội dung"
                headerStyle="text-align:center; height:50px"
                bodyStyle="text-align:center; "
                headerClass="align-items-center justify-content-center text-center overflow-hidden"
              >
              </Column>
              <Column
                header="Thời gian hoạt động"
                headerStyle="text-align:center;max-width:350px;height:50px"
                bodyStyle="text-align:center;max-width:350px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
                <template #body="data">
                  <div>
                    {{
                      moment(new Date(data.data.foundation_date)).format(
                        "DD/MM/YYYY"
                      )
                    }}
                    -
                    {{
                      moment(new Date(data.data.dissolution_date)).format(
                        "DD/MM/YYYY"
                      )
                    }}
                  </div>
                </template>
              </Column>
              <Column
                field="decision_date"
                header="Ngày quyết định"
                headerStyle="text-align:center ;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px; "
                headerClass="align-items-center justify-content-center text-center overflow-hidden"
              >
                <template #body="data">
                  <div v-if="data.data.created_date">
                    {{
                      moment(new Date(data.data.created_date)).format(
                        "HH:mm DD/MM/YYYY"
                      )
                    }}
                  </div>
                </template>
              </Column>

              <Column
                header=" "
                headerStyle="text-align:center;max-width:50px;height:50px"
                bodyStyle="text-align:center;max-width:50px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
                <template #body="Tem">
                  <div
                    v-if="
                      store.state.user.is_super == true ||
                      store.state.user.user_id == Tem.data.created_by ||
                      (store.state.user.role_id == 'admin' &&
                        store.state.user.organization_id ==
                          Tem.data.organization_id)
                    "
                  >
                    <Button
                      :v-tooltip.left="'Xem lịch sử'"
                      icon="pi pi-eye"
                      class="p-button-rounded p-button-outlined"
                      @click="editOrgHistory(Tem.data)"
                    />
                  </div>
                </template>
              </Column>
              <!-- <Column
                field="start_date"
                header="Ngày quyết định"
                headerStyle="text-align:center;max-width:100px;height:50px"
                bodyStyle="text-align:center;max-width:100px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
                <template #body="data">
                  <div v-if="data.data.decision_date">
                    {{
                      moment(new Date(data.data.decision_date)).format(
                        "DD/MM/YYYY"
                      )
                    }}
                  </div>
                </template>
              </Column> -->
            </DataTable>
          </div>
        </div>
      </div>
      <div   class="p-3">
        <Toolbar class="custoolbar">
          <template #end>
            <Button
              label="Huỷ"
              icon="pi pi-times"
              @click="closeSidebar"
              class="p-button-raised p-button-secondary mr-2"
            />
            <Button
              label="Cập nhật"
              icon="pi pi-save"
              @click="saveOrganization(!v$.$invalid)"
            />
          </template>
        </Toolbar>
      </div>
    </form>
  </Sidebar>

  <Dialog
    :header="headerOrgHistory"
    v-model:visible="displayOrgHistory"
    :style="{ width: '55vw' }"
    :maximizable="true"
    :autoZIndex="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2 mt-0">
        <div class="col-12 field flex align-items-center">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left"
              >Số quyết định <span class="redsao pl-1"> (*)</span></label
            >
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.decision_number"
              :style="{ backgroundColor: '#FEF9E7', fontWeight: 'bold' }"
              :class="{
                'p-invalid':
                  (org_history.decision_number == null ||
                    org_history.decision_number == '') &&
                  submitted,
              }"
              :disabled="isView"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left pl-3">Ngày quyết định</label>
            <Calendar
              style="width: calc(100% - 10rem)"
              id="icon"
              v-model="org_history.decision_date"
              :showIcon="true"
              :disabled="isView"
            />
          </div>
        </div>
        <div class="col-12 field flex align-items-center">
          <label class="w-10rem text-left">Nội dung</label>
          <Textarea
            :autoResize="true"
            rows="2"
            class="ip36"
            style="width: calc(100% - 10rem)"
            v-model="org_history.content"
            :disabled="isView"
          />
        </div>
        <div class="col-12 field flex align-items-center">
          <label class="w-10rem text-left"
            >Tên{{
              org_history.organization_type == 1 ? " phòng ban " : " đơn vị "
            }}</label
          >
          <InputText
            spellcheck="false"
            class="ip36"
            style="width: calc(100% - 10rem)"
            v-model="org_history.organization_name"
            :disabled="isView"
          />
        </div>

        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Tên tiếng Anh</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.organization_name_en"
              :disabled="isView"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem pl-3 text-left">Tên viết tắt</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.short_name"
              :disabled="isView"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Thời gian hoạt động</label>

            <Calendar
              style="width: calc(100% - 10rem)"
              id="icon"
              v-model="org_history.foundation_date"
              :showIcon="true"
              :disabled="isView"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left pl-3">Đến</label>

            <Calendar
              style="width: calc(100% - 10rem)"
              id="icon"
              v-model="org_history.dissolution_date"
              :showIcon="true"
              :disabled="isView"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Người đại diện</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.representative"
              :disabled="isView"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem pl-3 text-left">Mã doanh nghiệp</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.business_code"
              :disabled="isView"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Địa điểm làm việc</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.address"
              :disabled="isView"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem pl-3 text-left">Điện thoại</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.phone"
              :disabled="isView"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex align-items-center">
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem text-left">Địa điểm ĐKKD</div>
            <Textarea
              :autoResize="true"
              rows="1"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.address_registration"
              :disabled="isView"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem text-left pl-3">Website</div>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.is_url"
              :disabled="isView"
            />
          </div>
        </div>

        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Email</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.mail"
              :disabled="isView"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem pl-3 text-left">Fax</label>
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="org_history.fax"
              :disabled="isView"
            />
          </div>
        </div>
        <div class="col-12 flex align-items-center field">
          <div class="w-10rem text-left">Chức năng nhiệm vụ</div>
          <Textarea
            :autoResize="true"
            rows="1"
            class="ip36"
            style="width: calc(100% - 10rem)"
            v-model="org_history.feature"
            :disabled="isView"
          />
        </div>
        <div
          class="col-12 field text-lg font-bold"
          v-if="(listFilesS.length > 0 && isView) || !isView"
        >
          File đính kèm
        </div>
        <div class="w-full col-12 field">
          <FileUpload
            chooseLabel="Chọn File"
            :showUploadButton="false"
            :showCancelButton="false"
            :multiple="false"
            :maxFileSize="524288000"
            @select="onUploadFile"
            @remove="removeFile"
            :invalidFileSizeMessage="'{0}: Dung lượng File không được lớn hơn {1}'"
            v-if="!isView"
          >
            <template #empty>
              <p class="p-0 m-0 text-500">Kéo thả hoặc chọn File.</p>
            </template>
          </FileUpload>

          <div class="col-12 p-0" v-if="listFilesS.length > 0">
            <DataTable
              :value="listFilesS"
              filterDisplay="menu"
              filterMode="lenient"
              scrollHeight="flex"
              :showGridlines="true"
              :paginator="false"
              :row-hover="true"
              columnResizeMode="fit"
            >
              <Column field="code" header="  File đính kèm">
                <template #body="item">
                  <div
                    class="p-0 d-style-hover"
                    style="width: 100%; border-radius: 10px"
                  >
                    <div class="w-full flex align-items-center">
                      <div class="flex w-full text-900">
                        <div
                          v-if="item.data.is_image"
                          class="align-items-center flex"
                        >
                          <Image
                            :src="basedomainURL + item.data.file_path"
                            alt=""
                            width="70"
                            height="50"
                            style="
                              object-fit: contain;
                              border: 1px solid #ccc;
                              width: 70px;
                              height: 50px;
                            "
                            preview
                            class="pr-2"
                          />
                          <div class="ml-2" style="word-break: break-all">
                            {{ item.data.file_name }}
                          </div>
                        </div>
                        <div v-else>
                          <a
                            :href="basedomainURL + item.data.file_path"
                            download
                            class="w-full no-underline cursor-pointer text-900"
                          >
                            <div class="align-items-center flex">
                              <div>
                                <img
                                  :src="
                                    basedomainURL +
                                    '/Portals/Image/file/' +
                                    item.data.file_path.substring(
                                      item.data.file_path.lastIndexOf('.') + 1
                                    ) +
                                    '.png'
                                  "
                                  style="
                                    width: 70px;
                                    height: 50px;
                                    object-fit: contain;
                                  "
                                  alt=""
                                />
                              </div>
                              <div class="ml-2" style="word-break: break-all">
                                <div class="ml-2" style="word-break: break-all">
                                  <div style="word-break: break-all">
                                    {{ item.data.file_name }}
                                  </div>
                                  <div
                                    v-if="store.getters.user.is_super"
                                    style="
                                      word-break: break-all;
                                      font-size: 11px;
                                      font-style: italic;
                                    "
                                  >
                                    {{ item.data.organization_name }}
                                  </div>
                                </div>
                              </div>
                            </div>
                          </a>
                        </div>
                      </div>
                      <div
                        class="w-3rem align-items-center d-style-hover-1"
                        v-if="
                          store.getters.user.organization_id ==
                          item.data.organization_id
                        "
                      >
                        <Button
                          icon="pi pi-times"
                          class="p-button-rounded bg-red-300 border-none"
                          @click="deleteFileH(item.data)"
                        />
                      </div>
                    </div>
                  </div>
                </template>
              </Column>
            </DataTable>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Toolbar class="custoolbar">
        <template #start>
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == org_history.created_by ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id ==
                  org_history.organization_id &&
                isSaveOrgHistory == false &&
                !checkAddOrd)
            "
          >
            <Button
              label="Xóa"
              class="p-button-raised p-button-danger"
              icon="pi pi-trash"
              @click="delOrgHistory(org_history)"
            />
            <Button
              label="Sửa"
              class="p-button-raised"
              icon="pi pi-cog"
              @click="isEditOrgHistory()"
            />
          </div>
        </template>
        <template #end>
          <div>
            <Button
              label="Huỷ"
              icon="pi pi-times"
              @click="closeOrgHistory"
              class="p-button-raised p-button-secondary"
            />

            <Button label="Lưu" icon="pi pi-save" @click="saveOrgHistory()" />
          </div>
        </template>
      </Toolbar>
    </template>
  </Dialog>
  <Dialog
    :header="headerStructure"
    v-model:visible="displayStructure"
    :style="{ width: '45vw' }"
    :maximizable="true"
    :autoZIndex="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin chung</div>
        <div class="field p-0 col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left"
              >Số quyết định <span class="redsao pl-1"> (*)</span></label
            >
            <InputText
              spellcheck="false"
              class="ip36"
              style="width: calc(100% - 10rem)"
              v-model="department_merger.decision_number"
              :style="{ backgroundColor: '#FEF9E7', fontWeight: 'bold' }"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem pl-3 text-left"
              >Ngày {{ checkStructure ? "sáp nhập" : "giải thể" }}</label
            >
            <Calendar
              @blur="autoFillDate(department_merger, 'decision_date')"
              id="decision_date"
              :showIcon="true"
              :showOnFocus="false"
              autocomplete="off"
              v-model="department_merger.decision_date"
              style="width: calc(100% - 10rem)"
              placeholder="dd/mm/yyyy"
            />
          </div>
        </div>
        <div class="col-12 field p-0 flex align-items-center">
          <label class="w-10rem text-left"
            >Đơn vị {{ checkStructure ? "sáp nhập" : "giải thể" }}</label
          >
          <TreeSelect
            v-model="department_merger.current_department"
            :options="listTreeSelect"
            :placeholder="
              checkStructure ? 'Chọn đơn vị sáp nhập' : 'Chọn đơn vị giải thể'
            "
            style="width: calc(100% - 10rem)"
          />
        </div>
        <div
          class="col-12 field p-0 flex align-items-center"
          v-if="checkStructure"
        >
          <label class="w-10rem text-left">Đơn vị nhận</label>
          <TreeSelect
            v-model="department_merger.receive_department"
            :options="listTreeSelect"
            placeholder="Chọn đơn vị nhận"
            style="width: calc(100% - 10rem)"
          />
        </div>

        <div class="field p-0 col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="w-10rem text-left">Đơn vị ban hành</label>
            <TreeSelect
              v-model="department_merger.issuer"
              :options="listTreeSelect"
              placeholder="Chọn đơn vị ban hành"
              style="width: calc(100% - 10rem)"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem pl-3 text-left">Người ký</div>
            <div style="width: calc(100% - 10rem)">
              <DropdownProfile
                :model="department_merger.signer"
                :placeholder="'Chọn nhân sự'"
                :class="'w-full p-0'"
                :editable="false"
                optionLabel="profile_user_name"
                optionValue="code"
                :callbackFun="getProfileUser"
                :key_user="'signer'"
              />
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 text-lg font-bold">File đính kèm</div>
        <div class="w-full col-12 field p-0">
          <FileUpload
            chooseLabel="Chọn File"
            :showUploadButton="false"
            :showCancelButton="false"
            :multiple="false"
            :maxFileSize="524288000"
            @select="onUploadFile"
            @remove="removeFile"
            accept=".doc,.docx,.xlst,.xls"
            :invalidFileSizeMessage="'{0}: Dung lượng File không được lớn hơn {1}'"
          >
            <template #empty>
              <p class="p-0 m-0 text-500">Kéo thả hoặc chọn File.</p>
            </template>
          </FileUpload>

          <div class="col-12 p-0" v-if="listFilesS.length > 0">
            <DataTable
              :value="listFilesS"
              filterDisplay="menu"
              filterMode="lenient"
              scrollHeight="flex"
              :showGridlines="true"
              :paginator="false"
              :row-hover="true"
              columnResizeMode="fit"
            >
              <Column field="code" header="  File đính kèm">
                <template #body="item">
                  <div
                    class="p-0 d-style-hover"
                    style="width: 100%; border-radius: 10px"
                  >
                    <div class="w-full flex align-items-center">
                      <div class="flex w-full text-900">
                        <div
                          v-if="item.data.is_image"
                          class="align-items-center flex"
                        >
                          <Image
                            :src="basedomainURL + item.data.file_path"
                            alt=""
                            width="70"
                            height="50"
                            style="
                              object-fit: contain;
                              border: 1px solid #ccc;
                              width: 70px;
                              height: 50px;
                            "
                            preview
                            class="pr-2"
                          />
                          <div class="ml-2" style="word-break: break-all">
                            {{ item.data.file_name }}
                          </div>
                        </div>
                        <div v-else>
                          <a
                            :href="basedomainURL + item.data.file_path"
                            download
                            class="w-full no-underline cursor-pointer text-900"
                          >
                            <div class="align-items-center flex">
                              <div>
                                <img
                                  :src="
                                    basedomainURL +
                                    '/Portals/Image/file/' +
                                    item.data.file_path.substring(
                                      item.data.file_path.lastIndexOf('.') + 1
                                    ) +
                                    '.png'
                                  "
                                  style="
                                    width: 70px;
                                    height: 50px;
                                    object-fit: contain;
                                  "
                                  alt=""
                                />
                              </div>
                              <div class="ml-2" style="word-break: break-all">
                                <div class="ml-2" style="word-break: break-all">
                                  <div style="word-break: break-all">
                                    {{ item.data.file_name }}
                                  </div>
                                  <div
                                    v-if="store.getters.user.is_super"
                                    style="
                                      word-break: break-all;
                                      font-size: 11px;
                                      font-style: italic;
                                    "
                                  >
                                    {{ item.data.organization_name }}
                                  </div>
                                </div>
                              </div>
                            </div>
                          </a>
                        </div>
                      </div>
                      <div
                        class="w-3rem align-items-center d-style-hover-1"
                        v-if="
                          store.getters.user.organization_id ==
                          item.data.organization_id
                        "
                      >
                        <Button
                          icon="pi pi-times"
                          class="p-button-rounded bg-red-300 border-none"
                          @click="deleteFileH(item.data)"
                        />
                      </div>
                    </div>
                  </div>
                </template>
              </Column>
            </DataTable>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closeStructure"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Lưu" icon="pi pi-save" @click="saveStructure()" />
    </template>
  </Dialog>
  <Dialog
    :header="
      donvi.organization_type == 1 ? 'Cập nhật phòng ban ' : 'Cập nhật đơn vị '
    "
    v-model:visible="displayAddDonvi"
    :style="{ width: '860px' }"
    :maximizable="true"
    :autoZIndex="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >Tên{{ donvi.organization_type == 1 ? " phòng ban " : " đơn vị "
            }}<span class="redsao">(*)</span></label
          >
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="donvi.organization_name"
            :class="{ 'p-invalid': v$.organization_name.$invalid && submitted }"
          />
        </div>
        <small
          v-if="
            (v$.organization_name.required.$invalid && submitted) ||
            v$.organization_name.required.$pending.$response
          "
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3" v-if="donvi.organization_type == 0">{{
              v$.organization_name.required.$message
                .replace("Value", "Tên đơn vị")
                .replace("is required", "không được để trống")
            }}</span>
            <span class="col-10 pl-3" v-if="donvi.organization_type == 1">{{
              v$.organization_name.required.$message
                .replace("Value", "Tên phòng ban")
                .replace("is required", "không được để trống")
            }}</span>
          </div>
        </small>
        <small
          v-if="v$.organization_name.maxLength.$invalid && submitted"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3" v-if="donvi.organization_type == 0"
              >{{
                v$.organization_name.maxLength.$message.replace(
                  "The maximum length allowed is",
                  "Tên đơn vị không được vượt quá"
                )
              }}
              ký tự</span
            >
            <span class="col-10 pl-3" v-if="donvi.organization_type == 1"
              >{{
                v$.organization_name.maxLength.$message.replace(
                  "The maximum length allowed is",
                  "Tên phòng ban không được vượt quá"
                )
              }}
              ký tự</span
            >
          </div>
        </small>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Cấp quản lý</label>
          <TreeSelect
            @change="onChangeParent"
            class="col-10"
            v-model="selectCapcha"
            :options="treedonvis"
            :showClear="true"
            placeholder=""
            optionLabel="data.organization_name"
            optionValue="data.organization_id"
          >
          </TreeSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên tiếng Anh</label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="donvi.organization_name_en"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên viết tắt</label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="donvi.short_name"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Phân loại</label>
          <Dropdown
            class="col-10"
            v-model="donvi.organization_type"
            :options="tdorganization_types"
            optionLabel="text"
            optionValue="value"
          />
        </div>
        <div class="field col-12 md:col-12" v-if="donvi.organization_type == 0">
          <label class="col-2 text-left">Địa chỉ</label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="donvi.address"
          />
        </div>
        <div class="field col-12 md:col-12" v-if="donvi.organization_type == 0">
          <label class="col-2 text-left">Số điện thoại</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.phone"
          />
          <label class="col-2 text-left pl-4">Fax</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.fax"
          />
        </div>
        <div class="field col-12 md:col-12" v-if="donvi.organization_type == 0">
          <label class="col-2 text-left">Website</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.is_url"
          />
          <label class="col-2 text-left pl-4">Email</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.mail"
          />
        </div>
        <small
          v-if="v$.mail.email.$invalid && submitted && donvi.mail != null"
          class="p-error field col-12 md:col-12 mb-3 flex"
        >
          <div class="col-6"></div>
          <label class="col-2 text-left"></label>
          <span class="">{{
            v$.mail.email.$message.replace(
              "Value is not a valid email address",
              "Email không hợp lệ"
            )
          }}</span>
        </small>
        <div class="field col-12 md:col-12" v-if="donvi.organization_type == 0">
          <label class="col-2 text-left">Mã số doanh nghiệp</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.business_code"
          />
          <label class="col-2 text-left pl-4">Ngày thành lập</label>
          <Calendar
            class="col-4 ip36 p-0"
            id="icon"
            v-model="donvi.foundation_date"
            :showIcon="true"
          />
        </div>
        <div
          class="field col-12 md:col-12 flex"
          v-if="donvi.organization_type == 0"
        >
          <label class="col-2 text-left">Chức năng</label>
          <Textarea
            :autoResize="true"
            rows="5"
            class="col-10 py-2"
            v-model="donvi.feature"
          />
        </div>
        <div
          class="field col-12 md:col-12 flex"
          v-if="donvi.organization_type == 0"
        >
          <label class="col-2 text-left">Nhiệm vụ</label>
          <Textarea
            :autoResize="true"
            rows="5"
            class="col-10 py-2"
            v-model="donvi.mission"
          />
        </div>
        <div
          class="field col-12 md:col-12 flex"
          v-if="donvi.organization_type == 0"
        >
          <label class="col-2 text-left">Mô tả</label>
          <Textarea
            :autoResize="true"
            rows="5"
            class="col-10 py-2"
            v-model="donvi.description"
          />
        </div>
        <div
          class="field col-12 md:col-12 flex"
          v-if="donvi.organization_type == 0"
        >
          <label class="col-2">Logo</label>
          <div class="col-4 p-0">
            <div class="inputanh relative">
              <img
                @click="chonanh('AnhDonvi')"
                id="LogoDonvi"
                v-bind:src="
                  donvi.logo
                    ? basedomainURL + donvi.logo
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />
              <Button
                v-if="isDisplayAvt || donvi.logo"
                style="width: 1.5rem; height: 1.5rem"
                icon="pi pi-times"
                @click="delLogo"
                class="p-button-rounded absolute top-0 right-0 cursor-pointer"
              />
            </div>
            <input
              class="ipnone"
              id="AnhDonvi"
              type="file"
              accept="image/*"
              @change="handleFileUpload($event, 'LogoDonvi')"
            />
          </div>
          <label class="col-2 text-right">Ảnh nền</label>
          <div class="col-4 p-0">
            <div class="inputanh relative">
              <img
                @click="chonanh('AnhNen')"
                id="LogoNen"
                v-bind:src="
                  donvi.background_image
                    ? basedomainURL + donvi.background_image
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />
              <Button
                v-if="isDisplayNen || donvi.background_image"
                style="width: 1.5rem; height: 1.5rem"
                icon="pi pi-times"
                @click="delNen"
                class="p-button-rounded absolute top-0 right-0 cursor-pointer"
              />
            </div>
            <input
              class="ipnone"
              id="AnhNen"
              type="file"
              accept="image/*"
              @change="handleFileUpload($event, 'LogoNen')"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12" v-if="donvi.organization_type == 0">
          <label class="col-2 text-left">Tên phần mềm</label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="donvi.product_name"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Màu nền</label>
          <Button
            class="p-button-rounded p-button-outlined p-button-secondary col-2"
            :style="{
              backgroundColor: donvi.background_color,
              color: donvi.background_color ? 'transparent' : '#333',
              border: '1px solid #ccc',
            }"
            type="button"
            icon="pi pi-palette"
            @click="toggleColor($event, 'background_color')"
          />
          <OverlayPanel ref="opColor">
            <ColorPicker
              theme="dark"
              @changeColor="changeColor"
              :sucker-hide="true"
            />
          </OverlayPanel>
          <label class="col-3 text-center">Màu chữ</label>
          <Button
            class="p-button-rounded p-button-outlined p-button-secondary col-2"
            :style="{
              backgroundColor: donvi.text_color,
              color: donvi.text_color ? 'transparent' : '#333',
              border: '1px solid #ccc',
            }"
            type="button"
            icon="pi pi-palette"
            @click="toggleColor($event, 'text_color')"
          />
          <OverlayPanel ref="opColor">
            <ColorPicker
              theme="dark"
              @changeColor="changeColor"
              :sucker-hide="true"
            />
          </OverlayPanel>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="donvi.is_order" />
          <label class="col-2 text-right">Trạng thái</label>
          <InputSwitch v-model="donvi.status" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddDonvi"
        class="p-button-raised p-button-secondary"
      />
      <Button
        label="Cập nhật"
        icon="pi pi-save"
        @click="handleSubmit(!v$.$invalid)"
      />
    </template>
  </Dialog>
</template>
<style>
.p-treeselect-panel .p-treeselect-items-wrapper .p-tree {
  max-width: 660px !important;
}
</style>
<style scoped>
.d-design-org-hover:hover {
  background-color: #f0f8ff !important;
}
.d-design-org-hover:focus {
  background-color: #f0f8ff !important;
}
.text-error {
  color: red !important;
}
.classdonvi {
  background-color: aliceblue;
}
span.donvitrue {
  font-weight: 500;
}
.chip0 {
  background-color: #4285f4;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.chip1 {
  background-color: #689f38;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.chip2 {
  background-color: #607d8b;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.ipnone {
  display: none;
}
.inputanh {
  /* border: 1px solid #ccc; */
  width: 96px;
  height: 96px;
  cursor: pointer;
  padding: 1px;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
.donvi0 {
  font-weight: bold !important;
}
.item-hover:hover {
  color: #0099f3;
}
.row-active,
.active {
  color: rgb(13, 137, 236);
}
.c-red-600 {
  color: red;
}
</style>
<style lang="scss" scoped>
::v-deep(.col-12) {
  .p-inputswitch {
    top: 6px;
  }
}
::v-deep(.p-avatar) {
  img {
    object-fit: contain !important;
  }
}
</style>
