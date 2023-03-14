<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");

const swal = inject("$swal");
const isDynamicSQL = ref(false);
const displayAssets = ref(false);
const showListAssets = () => {
  displayAssets.value = true;
  if (issaveEmailGroup.value == false) {
    loadOrganization(store.getters.user.organization_id);
  }
};
const hideSelectDevice = () => {
  displayAssets.value = false;
};
const onSelectDevice = () => {
  displayAssets.value = false;
};
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const rules = {
  role_group_name: {
    required,
    $errors: [
      {
        $property: "role_group_name",
        $validator: "required",
        $message: "Tên nhóm duyệt không được để trống!",
      },
    ],
  },
};

const EmailGroup = ref({
  role_group_name: "",
  is_order: null,
  organization_id: null,
  status: null,
  is_type: null,
  department_id: null,
  is_default: null,
  type_approval: null,
  is_bydepartment: null,
});
const Email = ref({
  user_id: "",
  is_order: null,
  organization_id: null,
});
const selectedEmailGroups = ref();
const selectedEmail = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, EmailGroup);
const issaveEmailGroup = ref(false);
const issaveEmail = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = fileURL;
const checkDelList = ref(false);
const checkDelListEmail = ref(false);
const options = ref({
  IsNext: true,
  sort: "is_order desc",
  SearchText: null,
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  Status: null,
});
const optionsEmail = ref({
  IsNext: true,
  sort: "is_order desc",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: 0,
  Status: null,
});
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_ca_role_group_BrowseGroup_count",
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
        sttEmailGroup.value = options.value.totalRecords + 1;
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "Email.vue",
        logcontent: error.message,
        loai: 2,
      });
    });
};
const listDepartment = ref();
const datalistsD = ref();
const loadOrganization = (value) => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list",
            par: [
              { par: "pageno", va: 1 },
              { par: "pagesize", va: 1000000 },
              { par: "search", va: null },
              { par: "organization_type", va: null },
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
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        let obj = renderTree(
          data,
          "organization_id",
          "organization_name",
          "đơn vị",
        );
        datalistsD.value = [];
        listDepartment.value = [];
        datalistsD.value = obj.arrChils;
        listDepartment.value = obj.arrtreeChils;
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};
const loadUser = () => {
  listUsers.value = [];
  listDropdownUser.value = [];
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_dd",
            par: [
              { par: "search", va: options.value.SearchTextUser },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "role_id", va: null },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "department_id", va: null },
              { par: "position_id", va: null },
              { par: "pageno", va: 1 },
              { par: "pagesize", va: 10000 },
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

      data.forEach((element) => {
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id,
          role_name: element.role_name,
          organization_id: element.organization_id,
        });
        listUsers.value.push({ data: element, active: false });
      });
      listUsers.value = data;
      listDropdownUserGive.value = listDropdownUser.value;
      listDropdownUserCheck.value = listDropdownUser.value.filter(
        (x) => x.code != store.getters.user.user_id,
      );
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
const listDropdownUserGive = ref();
const listDropdownUserCheck = ref();
const listDropdownUser = ref();
const listUsers = ref([]);
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      m.label_order = m.IsOrder.toString();
      if (options.value.PageNo > 0) {
        m.STT = (options.value.PageNo - 1) * options.value.PageSize + i + 1;
      } else {
        m.STT = i + 1;
      }
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, index) => {
            em.label_order = mm.data.label_order + "." + em.is_order;
            em.STT = mm.data.STT + "." + (index + 1);
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
//Lấy dữ liệu ngôn ngữ
const loadData = (rf) => {
  if (store.state.user.is_super == true) {
    loadOrganization(store.getters.user.organization_id);
  }
  if (rf) {
    // if (isDynamicSQL.value) {
    //   loadDataSQL();
    //   return;
    // }
    if (rf) {
      loadCount();
    }
    axios
      .post(
        baseURL + "/api/DictionaryProc/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "doc_ca_role_group_BrowseGroup_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "user_id", va: store.state.user.user_id },
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
          controller: "NhomDuyet.vue",
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
const loadUserDepartment = (id) => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_ca_role_group_department_get",
            par: [{ par: "id", va: id }],
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
        datalistsD.value
          .filter((d) => d.data.organization_id == x.department_id)
          .forEach((k) => {
            k.data.userM = x.user_id;
          });
      });
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "Email.vue",
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

    options.value.id = datalists.value[datalists.value.length - 1].place_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].place_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  if (!isDynamicSQL.value) {
    loadData(true);
  } else {
    //  loadDataSQL();
  }
};
const isNextEmailPage = ref(false);
const onPageEmail = (event) => {
  if (event.rows != options.value.PageSize) {
    optionsEmail.value.PageSize = event.rows;
  }

  isNextEmailPage.value = true;
  if (event.page == 0) {
    //Trang đầu
    optionsEmail.value.id = null;
    optionsEmail.value.IsNext = true;
  } else if (event.page > optionsEmail.value.PageNo + 1) {
    //Trang cuối
    optionsEmail.value.id = -1;
    optionsEmail.value.IsNext = false;
  } else if (event.page > optionsEmail.value.PageNo) {
    //Trang sau
    optionsEmail.value.id =
      datalists.value[datalists.value.length - 1].place_id;
    optionsEmail.value.IsNext = true;
  } else if (event.page < optionsEmail.value.PageNo) {
    //Trang trước
    optionsEmail.value.id = datalists.value[0].place_id;
    optionsEmail.value.IsNext = false;
  }
  optionsEmail.value.PageNo = event.page;
  showEmails(options.value.groupid);
};
//Hiển thị dialog
const headerDialog = ref();
const headerAddEmail = ref();
const displayBasic = ref(false);
const displayEmail = ref(false);
const addEmail = (str) => {
  submitted.value = false;
  checkUser.value = [];
  issaveEmail.value = false;
  headerAddEmail.value = str;
  displayEmail.value = true;
};
const openBasic = (str) => {
  submitted.value = false;

  datalistsD.value = [];
  loadOrganization(store.getters.user.organization_id);
  EmailGroup.value = {
    role_group_name: "",
    is_order: sttEmailGroup.value,
    organization_id: null,
    status: true,
    department_id: null,
    is_default: false,
    type_approval: 0,
    is_bydepartment: false,
    is_type: 3,
  };
  EmailGroup.value.type_approval = 0;
  EmailGroup.value.organization_id = store.state.user.organization_id;
  issaveEmailGroup.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  EmailGroup.value = {
    role_group_name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};
const closeDialogEmail = () => {
  Email.value = {};
  displayEmail.value = false;
  showEmails(options.value.groupid);
};
//Thêm bản ghi
const User = ref();
const saveEmailGroup = (isFormValid) => {
  submitted.value = true;
  User.value = [];
  EmailGroup.value.department_id = EmailGroup.value.department_id
    ? parseInt(Object.keys(EmailGroup.value.department_id)[0])
    : null;
  if (EmailGroup.value.is_order == null)
    EmailGroup.value.is_order == sttEmailGroup.value;
  if (EmailGroup.value.status == null) {
    EmailGroup.value.status = false;
  }
  if (EmailGroup.value.is_bydepartment == true) {
    EmailGroup.value.type_approval = null;
  }
  if (EmailGroup.value.is_bydepartment == true) {
    datalistsD.value.forEach((x) => {
      let info = {
        department_id: x.data.organization_id,
        user_id: x.data.userM,
      };
      User.value.push(info);
    });
  }

  let formData = new FormData();
  formData.append("role", JSON.stringify(EmailGroup.value));
  formData.append("User", JSON.stringify(User.value));
  if (!isFormValid) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!issaveEmailGroup.value) {
    axios
      .post(
        baseURL + "/api/role_group_browse_group/Add_browse_groups",
        formData,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm nhóm duyệt thành công!");
          closeDialog();
          loadData(true);
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("role_group_name") == true
                ? "Tên nhóm duyệt không quá 500 ký tự!"
                : ms,
            icon: "error",
            confirmButtonText: "OK",
          });
          loadData(true);
        }
      })
      .catch(() => {
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
      .put(
        baseURL + "/api//role_group_browse_group/Update_browse_groups",
        formData,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa nhóm duyệt thành công!");
          loadData(true);
          closeDialog();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("role_group_name") == true
                ? "Tên nhóm duyệt không quá 500 ký tự!"
                : ms,
            icon: "error",
            confirmButtonText: "OK",
          });
          loadData(true);
        }
      })
      .catch(() => {
        swal.close();
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};

const saveEmail = (isFormValid) => {
  submitted.value = true;
  if (isFormValid == null) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isFormValid.forEach((x, i) => {
    let user = {
      role_group_id: emailGroupID.value,
      user_id: x,
      is_order:
        emailList.value != null &&
        emailList.value != [] &&
        emailList.value.length > 0
          ? emailList.value[emailList.value.length - 1].is_order + (i + 1)
          : i + 1,
      organization_id: store.state.user.organization_id,
    };
    axios
      .post(
        baseURL + "/api/role_group_users/Add_role_groups_user",
        user,
        config,
      )
      .then(() => {})
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  });
  swal.close();
  toast.success("Thêm người dùng thành công!");
  refreshEmail(emailGroupID.value);
  closeDialogEmail();
  showEmails(emailGroupID.value);
};
const sttEmailGroup = ref();
const sttEmail = ref();
//Sửa bản ghi
const editEmailGroup = (dataEmailGroup) => {
  loadUserDepartment(dataEmailGroup.role_group_id);
  submitted.value = false;
  EmailGroup.value = dataEmailGroup;
  headerDialog.value = "Sửa nhóm duyệt";
  issaveEmailGroup.value = true;
  displayBasic.value = true;
  {
    EmailGroup.value.organization_id = store.state.user.organization_id;
  }
};
//Xóa bản ghi
const delEmailGroup = (EmailGroup) => {
  if (EmailGroup != null) {
    deleteEmailOnDeleteGroup(EmailGroup.role_group_id);
  }
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm duyệt này không!",
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
            baseURL + "/api/role_group_browse_group/Delete_browse_groups",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: EmailGroup != null ? [EmailGroup.role_group_id] : -1,
            },
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá nhóm duyệt thành công!");
              if (
                (options.value.totalRecords - EmailGroup.length) % 2 == 0 &&
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

const delEmail = (Email) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá người dùng này không!",
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
          .delete(baseURL + "/api/role_group_users/Delete_Users", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Email != null ? [Email.role_group_user_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá người dùng thành công!");
              if (
                (optionsEmail.value.totalRecords - Email.length) % 2 == 0 &&
                optionsEmail.value.PageNo > 0
              ) {
                optionsEmail.value.PageNo = optionsEmail.value.PageNo - 1;
              }
              showEmails(emailGroupID.value);
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
//Sort
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    options.value.sort = "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  options.value.PageNo = 0;
  isDynamicSQL.value = true;
  // loadDataSQL();
};

const onSortEmail = (event) => {
  optionsEmail.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    optionsEmail.value.sort =
      "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  optionsEmail.value.PageNo = 0;
  // loadEmailSQL();
};
const isFirst = ref(true);
//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.role_group_id,
    TextID: value.role_group_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    store.state.user.role_id == "admin"
  ) {
    axios
      .put(baseURL + "/api/role_groups/Update_StatusGroup_Role", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật trạng thái hiển thị nhóm duyệt thành công!");
          loadData(true);
          closeDialog();
        } else {
          swal.fire({
            title: "Thông báo!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch(() => {
        swal.close();
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    swal.fire({
      title: "Thông báo!",
      text: "Bạn không có quyền chỉnh sửa! Chỉ có Quản trị viên đơn vị hoặc Quản trị viên hệ thống mới có quyền chỉnh sửa mục này",
      icon: "error",
      confirmButtonText: "OK",
    });
    loadData(true);
  }
};
const onCheckBoxDefault = (value) => {
  let data = {
    IntID: value.role_group_id,
    TextID: value.role_group_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.is_default,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    store.state.user.role_id == "admin"
  ) {
    axios
      .put(
        baseURL + "/api/role_group_browse_group/Update_DefaultGroup_Role",
        data,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật trạng thái mặc định nhóm duyệt thành công!");
          loadData(true);
          closeDialog();
        } else {
          swal.fire({
            title: "Thông báo!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch(() => {
        swal.close();
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    swal.fire({
      title: "Thông báo!",
      text: "Bạn không có quyền chỉnh sửa! Chỉ có Quản trị viên đơn vị hoặc Quản trị viên hệ thống mới có quyền chỉnh sửa mục này",
      icon: "error",
      confirmButtonText: "OK",
    });
    loadData(true);
  }
};
const loadingUser = ref(false);
const onFilterUserDropdown = (value) => {
  loadingUser.value = true;
  if (value.organization_id == 1)
    listDropdownUserGive.value = listDropdownUser.value;
  else
    listDropdownUserGive.value = listDropdownUser.value.filter(
      (x) => x.department_id == value.organization_id,
    );
  loadingUser.value = false;
};

//Xóa nhiều
const deleteEmailList = () => {
  let listId = new Array(selectedEmail.value.length);
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
        selectedEmail.value.forEach((item) => {
          listId.push(item.role_group_user_id);
        });
        axios
          .delete(baseURL + "/api/role_group_users/Delete_Users", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách thành công!");
              checkDelListEmail.value = false;
              if (
                (optionsEmail.value.totalRecords - listId.length) % 2 == 0 &&
                optionsEmail.value.PageNo > 0
              ) {
                optionsEmail.value.PageNo = optionsEmail.value.PageNo - 1;
              }
              loadData(true);
              showEmails(emailGroupID.value);
              refreshEmail(emailGroupID.value);
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

const deleteList = () => {
  let listId = new Array(selectedEmailGroups.value.length);
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
        selectedEmailGroups.value.forEach((item) => {
          listId.push(item.role_group_id);
        });
        listId.forEach((element) => {
          deleteEmailOnDeleteGroup(element);
        });
        axios
          .delete(
            baseURL + "/api//role_group_browse_group/Delete_browse_groups",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            },
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách nhóm duyệt thành công!");
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
//Filter

const isType = ref([
  { name: "Duyệt một trong nhiều", code: 0 },
  { name: "Duyệt tuần tự", code: 1 },
]);
const filterPhanloai = ref();
const filterTrangthai = ref();
const styleObj1 = ref();
const isShowEmail = ref(false);
const emailList = ref([]);
const emailGroupID = ref();
const refreshEmail = () => {
  selectedEmail.value = [];
  filterPhanloai.value = "";
  optionsEmail.value = {
    PageNo: 0,
    PageSize: 20,
  };
  filterTrangthai.value = "";
  styleObj1.value = "";
  optionsEmail.value.SearchText = "";
  firstEmail.value = 0;
  loadEMail();
};
const showEmails = (value) => {
  emailList.value = [];
  EmailGrName.value = !value.role_group_name
    ? EmailGrName.value
    : value.role_group_name;
  options.value.loading = true;
  options.value.groupid = !value.role_group_id ? value : value.role_group_id;
  loadEMail();
  isShowEmail.value = true;
  //  document.getElementById(value).style.backgroundColor = "red";
  emailGroupID.value = !value.role_group_id ? value : value.role_group_id;
  if (isShowEmail.value == true) options.value.loading = false;
  else options.value.loading = true;
  LoadUser();
};
const loadEMail = () => {
  optionsEmail.value.loading = true;
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "role_group_user_count",
            par: [{ par: "id", va: options.value.groupid }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      optionsEmail.value.totalRecords = data[0].totalRecords;
      sttEmail.value = data[0].totalRecords + 1;
    });
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "role_group_user_list",
            par: [
              { par: "group_id", va: options.value.groupid },
              { par: "pageno", va: optionsEmail.value.PageNo },
              { par: "pagesize", va: optionsEmail.value.PageSize },
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
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT =
            optionsEmail.value.PageNo * options.value.PageSize + i + 1;
        });
      }
      emailList.value = data;
      isShowEmail.value = true;
      optionsEmail.value.loading = false;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      optionsEmail.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "Email.vue",
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
watch(selectedEmailGroups, () => {
  if (selectedEmailGroups.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
watch(selectedEmail, () => {
  if (selectedEmail.value.length > 0) {
    checkDelListEmail.value = true;
  } else {
    checkDelListEmail.value = false;
  }
});
const styleObj = ref();
const EmailGrName = ref();
const hideall = () => {
  displayEmail.value = false;
  isShowEmail.value = false;
  loadData(true);
};

const prevPage = () => {
  isShowEmail.value = false;
  selectedEmail.value = [];
};
const first = ref(0);
const firstEmail = ref(0);
const refresh = () => {
  options.value = {
    PageNo: 0,
    PageSize: 20,
  };
  options.value.loading = true;
  filterPhanloai.value = "";
  filterTrangthai.value = "";
  isDynamicSQL.value = false;
  selectedEmailGroups.value = [];
  loadData(true);
  styleObj.value = "";
  options.value.SearchText = "";
  first.value = 0;
};
const deleteEmailOnDeleteGroup = (id) => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_ca_role_group",
            par: [{ par: "id", va: id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then(() => {})
    .catch(() => {});
};
const width = ref(window.screen.width);
//upload section
// -------------------Email----------------
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const ListUser = ref();
const searchUser = ref();
const checkUser = ref();
const LoadUser = () => {
  ListUser.value = [];
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "role_group_getUsertoList",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "search", va: searchUser.value },
              { par: "gr_id", va: emailGroupID.value },
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
      ListUser.value = data;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      addLog({
        title: "Lỗi Console loadData",
        controller: "Email.vue",
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
onMounted(() => {
  loadData(true);
  loadOrganization(store.getters.user.organization_id);
  loadUser();
  return {};
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      v-model:first="first"
      :lazy="true"
      @page="onPage($event)"
      @sort="onSort($event)"
      :value="datalists"
      :loading="options.loading"
      :paginator="true"
      :rows="options.PageSize"
      :totalRecords="options.totalRecords"
      dataKey="role_group_id"
      :rowHover="true"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      v-model:selection="selectedEmailGroups"
      :class="'w-full'"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-at"> </i> Danh sách nhóm duyệt ({{
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
              @click="openBasic('Thêm mới nhóm duyệt')"
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
        headerStyle="text-align:center;max-width:5rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:5rem; "
        class="align-items-center justify-content-center text-center"
        v-if="store.state.user.is_super == true || store.state.user.is_admin"
      >
      </Column>
      <Column
        field="STT"
        header="STT"
        :sortable="false"
        headerStyle="text-align:center;max-width:8rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:8rem; "
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="role_group_name"
        header="Tên nhóm duyệt"
        :sortable="false"
        headerStyle="height:3.125rem"
        bodyStyle=" "
      >
      </Column>
      <Column
        field="type_approval"
        header="Loại duyệt"
        headerStyle="text-align:center;max-width:20rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:20rem; "
        class="align-items-center justify-content-center text-center"
        ><template #body="data">
          <span v-if="data.data.type_approval == 0">Duyệt một trong nhiều</span>
          <span v-if="data.data.type_approval == 1">Duyệt tuần tự</span>
          <span v-if="data.data.type_approval == null"
            >Duyệt theo phòng ban</span
          >
        </template>
      </Column>
      <Column
        field="email_count"
        header="Thành viên"
        headerStyle="text-align:center;max-width:150px;height:3.125rem"
        bodyStyle="text-align:center;max-width:150px; "
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div v-if="data.data.type_approval != null">
            <Button
              :id="data.data.role_group_id"
              class="p-button-rounded p-button-secondary"
              @click="showEmails(data.data)"
            >
              {{ data.data.email_count }}</Button
            >
          </div>
        </template>
      </Column>
      <Column
        field="status"
        header="Hiển thị"
        headerStyle="text-align:center;max-width:10rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:10rem; "
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
        field="is_default"
        header="Mặc định"
        headerStyle="text-align:center;max-width:10rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:10rem; "
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="data.data.is_default"
            v-model="data.data.is_default"
            @click="onCheckBoxDefault(data.data)"
          />
        </template>
      </Column>

      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:15rem;height:3.125rem;min-width:9.375rem;"
        bodyStyle="text-align:center;max-width:15rem ;min-width:9.375rem"
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
              @click="editEmailGroup(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="delEmailGroup(data.data, true)"
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
            src="../../assets/background/nodata.png"
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
    class="main-layout p-sidebar-lg py-0 overflow-hidden true flex-grow-1 p-2"
    :style="
      width < 1900
        ? 'width: 45vw; min-height: 100vh !important'
        : 'width: 35vw; min-height: 100vh !important'
    "
    :showCloseIcon="false"
    @hide="hideall()"
  >
    <div
      :style="
        width < 1900
          ? 'height: 100vh;max-height: 95vh !important;over-follow: hidden !important;margin-top: 1rem;'
          : 'height: 100vh;max-height: 97vh !important;over-follow: hidden !important;margin-top: 1rem;'
      "
    >
      <DataTable
        :lazy="true"
        @page="onPageEmail($event)"
        @filter="onFilterEmail($event)"
        @sort="onSortEmail($event)"
        :value="emailList"
        :loading="optionsEmail.loading"
        :paginator="true"
        :rows="optionsEmail.PageSize"
        :totalRecords="optionsEmail.totalRecords"
        dataKey="user_id"
        :rowHover="true"
        v-model:filters="filters"
        filterDisplay="menu"
        :showGridlines="true"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        :globalFilterFields="['email_name', 'full_name']"
        v-model:selection="selectedEmail"
        class="w-full"
        :style="'min-height: 92vh !important'"
        v-model:first="firstEmail"
      >
        <template #header>
          <h3 class="module-title mt-0 ml-1 mb-2">
            <i class="pi pi-at"></i>
            Danh sách người dùng ({{ optionsEmail.totalRecords }}) - Nhóm duyệt:
            {{ EmailGrName }}
          </h3>

          <Toolbar class="w-full custoolbar">
            <template #end>
              <Button
                label="Quay lại"
                icon="pi pi-arrow-left"
                class="mr-2 p-button-outlined"
                @click="prevPage"
              />
              <Button
                v-if="checkDelListEmail"
                @click="deleteEmailList($event)"
                label="Xóa"
                icon="pi pi-trash"
                class="mr-2 p-button-danger"
              />
              <Button
                @click="addEmail('Thêm người dùng')"
                label="Thêm mới"
                icon="pi pi-plus"
                class="mr-2"
                v-tooltip="'Thêm mới'"
              />
              <Button
                @click="refreshEmail()"
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                v-tooltip="'Tải lại'"
              />
            </template>
          </Toolbar>
        </template>
        <Column
          selectionMode="multiple"
          headerStyle="text-align:center;max-width:5rem;height:50px"
          bodyStyle="text-align:center;max-width:5rem;"
          class="align-items-center justify-content-center text-center"
          v-if="
            store.state.user.is_super == true ||
            store.state.user.role_id == 'admin'
          "
        >
        </Column>
        <Column
          field="is_order"
          header="STT"
          headerStyle="text-align:center;max-width:5rem;height:50px"
          bodyStyle="text-align:center;max-width:5rem;"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <Column
          field="full_name"
          header="Thông tin"
          headerStyle="justify-content:center;align-items:center;text-align:center;height:50px"
          bodyStyle=""
          class=""
        >
          <template #body="data">
            <div class="flex row col-12 p-0 m-0">
              <div class="flex col-3 p-0 m-0 format-center">
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-bind:label="
                    data.data.avt
                      ? ''
                      : data.data.full_name.split(' ').at(-1).substring(0, 1)
                  "
                  v-bind:image="basedomainURL + data.data.avt"
                  style="color: #ffffff; cursor: pointer"
                  :style="{
                    background: bgColor[data.data.is_order % 7],
                    border: '2px solid' + bgColor[data.data.is_order % 7],
                  }"
                  class="myTextAvatar"
                  size="xlarge"
                  shape="circle"
                />
              </div>
              <div class="flex col-9 p-0 m-0">
                <div>
                  <span class="font-bold">{{ data.data.full_name }}</span>
                  <br />

                  <span class="font-bold">{{ data.data.positions }}</span>
                  <br v-if="data.data.organization_name != null" />
                  <span>
                    {{
                      data.data.organization_name
                        ? data.data.organization_name
                        : ""
                    }}
                  </span>

                  <br v-if="data.data.department_name != null" />
                  <span>
                    {{
                      data.data.department_name ? data.data.department_name : ""
                    }}
                  </span>
                  <br v-if="data.data.phone != null" />
                  <span>
                    {{ data.data.phone ? data.data.phone : "" }}
                  </span>
                </div>
              </div>
            </div>
          </template>
        </Column>

        <Column
          header="Chức năng"
          headerStyle="text-align:center;max-width:7em;height:50px"
          bodyStyle="text-align:center;max-width:7rem;"
          class="align-items-center justify-content-center text-center"
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
                @click="delEmail(data.data, true)"
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
              src="../../assets/background/nodata.png"
              height="144"
            />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
      </DataTable>
    </div>
  </Sidebar>
  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="false"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Nhóm duyệt<span class="redsao"> (*) </span></label
          >
          <InputText
            v-model="EmailGroup.role_group_name"
            spellcheck="false"
            class="col-8 ip36 p-0 px-2"
            :class="{ 'p-invalid': v$.role_group_name.$invalid && submitted }"
          />
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12 p-0 mb-2"
        >
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v$.role_group_name.$invalid && submitted) ||
              v$.role_group_name.$pending.$response
            "
            class="col-9 p-error p-0"
          >
            <span class="col-12 p-0">{{
              v$.role_group_name.required.$message
                .replace("Value", "Tên nhóm duyệt")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div
          class="field col-12 md:col-12"
          v-if="EmailGroup.is_bydepartment == false"
        >
          <label class="col-3 text-left p-0">Loại duyệt </label>
          <Dropdown
            class="col-8 p-0 m-0"
            v-model="EmailGroup.type_approval"
            :options="isType"
            optionLabel="name"
            optionValue="code"
            placeholder="Loại duyệt"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 p-0 text-left">Phòng ban</label>
          <TreeSelect
            v-model="EmailGroup.department_id"
            :options="listDepartment"
            :showClear="true"
            :max-height="280"
            class="col-8 p-0"
            placeholder="Chọn phòng ban "
            optionLabel="data.organization_name"
            optionValue="data.department_id"
            panelClass="d-design-dropdown"
          >
          </TreeSelect>
        </div>

        <div
          style="display: flex"
          class="col-12 field md:col-12"
        >
          <div class="field col-6 md:col-6 p-0">
            <label class="col-6 text-left p-0">STT </label>
            <InputNumber
              v-model="EmailGroup.is_order"
              class="col-6 ip36 p-0"
            />
          </div>
          <div class="field col-1 md:col-1 p-0"></div>
          <div class="field col-5 md:col-5 p-0 format-left">
            <label
              style="vertical-align: text-bottom !important"
              class="col-6 text-left p-0"
              >Trạng thái
            </label>
            <InputSwitch
              v-model="EmailGroup.status"
              class="col-6 format-left"
            />
          </div>
        </div>
        <div
          style="display: flex"
          class="col-12 field md:col-12"
        >
          <div class="field col-6 md:col-6 p-0">
            <label
              style="vertical-align: text-bottom"
              class="col-6 text-left p-0"
              >Nhóm duyệt mặc định
            </label>
            <InputSwitch
              v-model="EmailGroup.is_default"
              class="col-6"
            />
          </div>
          <div class="field col-1 md:col-1 p-0"></div>

          <div class="field col-5 md:col-5 p-0">
            <label
              class="col-6 text-left p-0"
              style="vertical-align: text-bottom"
            >
              Duyệt theo phòng ban
            </label>
            <InputSwitch
              v-model="EmailGroup.is_bydepartment"
              class="col-6"
            />
          </div>
        </div>
        <div
          v-if="EmailGroup.is_bydepartment == true"
          class="field p-0 pb-2 col-12 md:col-12 flex"
        >
          <div
            class="col-6 p-0 flex align-items-center cursor-pointer text-blue-500"
            @click="showListAssets"
          >
            <i class="pi pi-plus-circle pr-2"></i> Cấu hình người duyệt theo
            phòng ban
          </div>
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
        @click="saveEmailGroup(!v$.$invalid)"
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerAddEmail"
    v-model:visible="displayEmail"
    :style="{ width: '25vw' }"
    :closable="false"
  >
    <form>
      <div class="row p-0 m-0">
        <div class="col-12 p-0 m-0 py-2 h-full">
          <InputText
            v-model="searchUser"
            @keyup="LoadUser"
            type="text"
            spellcheck="false"
            placeholder="Nhập tên người dùng..."
            class="col-12 h-full"
            row="2"
          />
        </div>
      </div>

      <div class="row p-0 m-0">
        <div class="col-12 p-0 m-0">
          <ScrollPanel style="width: 100%; height: 50vh">
            <div
              v-for="(p, index) in ListUser"
              :key="p"
            >
              <div
                class="row p-0 m-0 flex py-2"
                :class="
                  checkUser != null && checkUser.includes(p.user_id)
                    ? 'user-hover'
                    : ''
                "
              >
                <div class="flex col-1 p-0 m-0 format-center">
                  <Checkbox
                    :value="p.user_id"
                    v-model="checkUser"
                  />
                </div>
                <div class="flex col-4 p-0 m-0 format-center">
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-bind:label="
                      p.avatar
                        ? ''
                        : p.full_name.split(' ').at(-1).substring(0, 1)
                    "
                    v-bind:image="basedomainURL + p.avatar"
                    style="color: #ffffff; cursor: pointer"
                    :style="{
                      background: bgColor[index % 7],
                      border: '2px solid' + bgColor[index % 7],
                    }"
                    class=""
                    size="large"
                    shape="circle"
                  />
                </div>
                <div class="flex col-7 p-0 m-0">
                  <div>
                    <span class="font-bold text-xl">{{ p.full_name }}</span>
                    <br />
                    <span class="font-bold">{{ p.tenChucVu }}</span>
                    <br v-if="p.organization_name != null" />
                    <span>
                      {{ p.organization_name ? p.organization_name : "" }}
                    </span>

                    <br v-if="p.department_name != null" />
                    <span>
                      {{ p.department_name ? p.department_name : "" }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </ScrollPanel>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogEmail"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveEmail(checkUser)"
      />
    </template>
  </Dialog>
  <Dialog
    header="Cập nhật nhóm duyệt phòng ban"
    v-model:visible="displayAssets"
    :maximizable="true"
    :style="{ width: '50vw' }"
  >
    <div>
      <div
        class="true flex-grow-1 p-2"
        id="scrollTop"
      >
        <div class="grid p-0">
          <div class="col-12 field format-center">
            <TreeTable
              :expandedKeys="expandedKeys"
              :value="datalistsD"
            >
              <Column
                class="w-7"
                field="organization_name"
                :expander="true"
              >
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-building pr-2"></i> Phòng ban
                  </div>
                </template>
              </Column>
              <Column class="w-5">
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-user pr-2"></i> Người duyệt
                  </div>
                </template>
                <template #body="data">
                  <div class="w-full flex">
                    <Dropdown
                      v-model="data.node.data.userM"
                      panelClass="d-design-dropdown"
                      :options="listDropdownUserGive"
                      :filter="true"
                      optionLabel="name"
                      optionValue="code"
                      class="w-full"
                      placeholder="Chọn người duyệt "
                      :showClear="true"
                      :virtualScrollerOptions="{
                        lazy: true,
                        itemSize: 1,
                        showLoader: true,
                        loading: loadingUser,
                        delay: 250,
                      }"
                      @click="onFilterUserDropdown(data.node.data)"
                    >
                      <template #value="slotProps">
                        <div
                          class="country-item country-item-value flex align-items-center"
                          v-if="slotProps.value"
                        >
                          <Avatar
                            v-bind:label="
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value,
                              )[0].avatar
                                ? ''
                                : listDropdownUser
                                    .filter((x) => x.code == slotProps.value)[0]
                                    .name.substring(
                                      listDropdownUser
                                        .filter(
                                          (x) => x.code == slotProps.value,
                                        )[0]
                                        .name.lastIndexOf(' ') + 1,
                                      listDropdownUser
                                        .filter(
                                          (x) => x.code == slotProps.value,
                                        )[0]
                                        .name.lastIndexOf(' ') + 2,
                                    )
                            "
                            :image="
                              basedomainURL +
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value,
                              )[0].avatar
                            "
                            class="w-2rem h-2rem mr-2"
                            size="large"
                            :style="
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value,
                              )[0].avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[
                                    listDropdownUser.filter(
                                      (x) => x.code == slotProps.value,
                                    )[0].name.length % 7
                                  ]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                          <div>
                            {{
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value,
                              )[0].name
                            }}
                          </div>
                        </div>
                        <span v-else>
                          {{ slotProps.placeholder }}
                        </span>
                      </template>
                      <template #option="slotProps">
                        <div class="country-item flex align-items-center">
                          <Avatar
                            v-bind:label="
                              slotProps.option.avatar
                                ? ''
                                : slotProps.option.name.substring(
                                    slotProps.option.name.lastIndexOf(' ') + 1,
                                    slotProps.option.name.lastIndexOf(' ') + 2,
                                  )
                            "
                            :image="basedomainURL + slotProps.option.avatar"
                            class="w-3rem h-3rem"
                            size="large"
                            :style="
                              slotProps.option.avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[slotProps.option.name.length % 7]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                          <div class="pt-1 pl-2">
                            {{ slotProps.option.name }}
                          </div>
                        </div>
                      </template>
                    </Dropdown>
                  </div>
                </template>
              </Column>
            </TreeTable>
          </div>
        </div>
      </div>

      <div
        class="p-0"
        id="scrollDM"
      >
        <Toolbar class="p-2 surface-0 border-none">
          <template #end>
            <Button
              @click="hideSelectDevice()"
              label="Hủy"
              icon="pi pi-times"
              class="mr-2 p-button-outlined"
            />
            <Button
              @click="onSelectDevice()"
              label="Chọn"
              icon="pi pi-check"
              autofocus
            />
          </template>
        </Toolbar>
      </div>
    </div>
  </Dialog>
</template>

<style scoped>
.user-hover {
  background-color: var(--blue-100);
}
.email {
  background-color: #2196f3 !important;
  color: #fff !important;
  border: 1px solid #5ca7e3 !important;
}
.p-paginator .p-paginator-pages .p-paginator-page.p-highlight {
  background: #2196f3 !important;
  border-color: #fde3e3;
  color: #574949;
}
</style>
<style lang="scss" scoped>
.p-treeselect-panel {
  max-width: 30vw !important;
}
.p-treeselect-panel .p-treeselect-items-wrapper .p-tree {
  max-height: 17vh !important;
}
.p-dropdown-item {
  white-space: normal !important;
}
.myTextAvatar .p-avatar-text {
  font-size: 2rem !important;
}
</style>
