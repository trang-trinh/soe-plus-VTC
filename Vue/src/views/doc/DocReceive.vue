<script setup>
import { onMounted, ref, inject } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import DocList from "../../components/doc/DocList.vue";
import DocDetail from "../../components/doc/DocDetail.vue";
import DocSignIframe from "../../components/doc/DocSignIframe.vue";
import DocFilter from "../../components/doc/DocFilter.vue";
import DocAdditionalCount from "../../components/doc/DocAdditionalCount.vue";
import DocShareFile from "../../components/doc/DocShareFile.vue";
import DocLinkTask from "../../components/doc/DocLinkTask.vue";
import TreeSelectCustom from "../../components/doc/TreeSelectCustom.vue";
import DocConnectSendDialog from "../../components/doc/DocConnectSendDialog.vue";
import DocSelectReceivePlaceOnline from "../../components/doc/DocSelectIssuePlace.vue";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import treeuser from "../../components/user/treeuser.vue";
import { encr } from "../../util/function";
//end
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const router = inject("router");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const options = ref({
  search: null,
  organization_id: store.getters.user.organization_id,
  department_id: store.getters.user.department_id
})
const isLoaded = ref(false);
const isLoading = ref(false);
// Modal Tree User
const selectedUser = ref([]);
const is_one = ref(false);
const is_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const showModalUser = (one, type) => {
  selectedUser.value = [];
  headerDialogUser.value = "Chọn người dùng";
  is_one.value = one;
  is_type.value = type;
  displayDialogUser.value = true;
};
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
const choiceUser = () => {
  switch (is_type.value) {
    case 0:
      var notexist = selectedUser.value.filter(
        (a) =>
          publish_item.value.users.findIndex(

            (b) => b === a["user_id"]
          ) === -1
      );
      publish_item.value.users = publish_item.value.users.concat(notexist.map(x => x.user_id));
      break;
    case 1:
    if(selectedDoc.value.is_drafted){
        followperson_item.value.hosts = selectedUser.value[0].user_id;
      }
      else{
        var notexist = selectedUser.value.filter(
        (a) =>
          followperson_item.value.hosts.findIndex(

            (b) => b === a["user_id"]
          ) === -1
      );
      followperson_item.value.hosts = followperson_item.value.hosts.concat(notexist.map(x => x.user_id));
      }
      break;
    case 2:
      var notexist = selectedUser.value.filter(
        (a) =>
          followperson_item.value.trackers.findIndex(

            (b) => b === a["user_id"]
          ) === -1
      );
      followperson_item.value.trackers = followperson_item.value.trackers.concat(notexist.map(x => x.user_id));
      break;
    case 3:
      var notexist = selectedUser.value.filter(
        (a) =>
          followperson_item.value.seetoknow_users.findIndex(

            (b) => b === a["user_id"]
          ) === -1
      );
      followperson_item.value.seetoknow_users = followperson_item.value.seetoknow_users.concat(notexist.map(x => x.user_id));
      break;
    case 4:
    approval_item.value.receive_by = selectedUser.value[0].user_key;
      break;
      case 5:
      var notexist = selectedUser.value.filter(
        (a) =>
          followgroup_item.value.trackers.findIndex(

            (b) => b === a["user_id"]
          ) === -1
      );
      followgroup_item.value.trackers = followgroup_item.value.trackers.concat(notexist.map(x => x.user_id));
      case 6:
      var notexist = selectedUser.value.filter(
        (a) =>
          followdepartment_item.value.trackers.findIndex(

            (b) => b === a["user_id"]
          ) === -1
      );
      followdepartment_item.value.trackers = followdepartment_item.value.trackers.concat(notexist.map(x => x.user_id));
      break;
      case 7:
      transferstamp_item.value.user_id = selectedUser.value[0].user_id;
      break;
  }
  closeDialogUser();
};
//
const all_users = ref([]);
const loadUsers = () => {
  axios
    .post(
      baseURL
      //baseUrl
      + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_user_list",
        par: [
          { par: "organization_id", va: options.value.organization_id },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0]) {
        all_users.value = data[0];
      }
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
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
}
// reload component
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
// Common variable
const isFirst = ref(true);
const allowDelDoc = ref(false);
// defined Doc Item variable
const headerDoc = ref();
const isAddDoc = ref(true);
const displayDocSohoa = ref(false);
const displayDocDuthao = ref(false);
const doc = ref({
  compendium: null,
  notes: "",
});
const selectedDoc = ref({});
const submitted = ref(false);
const rules = {
  // doc_code: {
  //   required,
  //   $errors: [
  //     {
  //       $property: "doc_code",
  //       $validator: "required",
  //       $message: "Số ký hiệu văn bản không được để trống!",
  //     },
  //   ],
  // },
  dispatch_book_code: {
    required,
    $errors: [
      {
        $property: "dispatch_book_code",
        $validator: "required",
        $message: "Số vào sổ không được để trống!",
      },
    ],
  },
  // doc_date: {
  //   required,
  //   $errors: [
  //     {
  //       $property: "doc_date",
  //       $validator: "required",
  //       $message: "Ngày văn bản không được để trống!",
  //     },
  //   ],
  // },
  issue_place: {
    required,
    $errors: [
      {
        $property: "issue_place",
        $validator: "required",
        $message: "Nơi ban hành không được để trống!",
      },
    ],
  },
  compendium: {
    required,
    $errors: [
      {
        $property: "compendium",
        $validator: "required",
        $message: "Trích yếu không được để trống!",
      },
    ],
  }
};
const rules_duthao = {
  compendium: {
    required,
    $errors: [
      {
        $property: "compendium",
        $validator: "required",
        $message: "Trích yếu không được để trống!",
      },
    ],
  }
}
const v$ = useVuelidate(rules, doc);
const v$_dt = useVuelidate(rules_duthao, doc);
const v$_stamp = useVuelidate(rules, doc);
// utils
const convertDatefromDB = (obj) => {
  for (var key in obj) {
    let isnum = /^\d+$/.test(obj[key]);
    if (!isnum && moment(obj[key], moment.ISO_8601, true).isValid()) {
      obj[key] = new Date(obj[key]);
    }
  }
}
const autoFillDate = (model,prop_name) => {
    var ip_val = document.getElementById(prop_name).value;
    if(ip_val.length === 2 && ip_val > 0){
        var today = new Date();
        var dd = today.getDate();
        var mm = today.getMonth() + 1;
        var yyyy = today.getFullYear();
        if(dd < 10) dd = '0' + dd;
        if(mm < 10) mm = '0' + mm;

        model[prop_name] = ip_val + '/' + mm + '/' + yyyy;
        ip_val = "";
        if(!moment(model[prop_name], "DD/MM/YYYY", true).isValid()){
          model[prop_name] = dd + '/' + mm + '/' + yyyy;
        };
    }
}
// defined function doc
const defined_doc_functions = ref([
  {
    id: '1',
    is_order: 1,
    typeBtn: 'basic',
    label: 'Thêm mới',
    icon: 'pi pi-fw pi-plus-circle',
    items: [
      {
        label: 'Văn bản đến',
        icon: 'pi pi-fw pi-plus',
        command: () => {
          openModalSohoa(1);
        }
      },
      {
        label: 'Văn bản đi',
        icon: 'pi pi-fw pi-plus',
        command: () => {
          openModalSohoa(2);
        }
      },
      {
        label: 'Văn bản nội bộ',
        icon: 'pi pi-fw pi-plus',
        command: () => {
          openModalSohoa(3);
        }
      },
      // {
      //   label: 'Nhận dạng AI/OCR',
      //   icon: 'pi pi-fw pi-plus',
      // }
    ]
  },
  {
    id: '2',
    is_order: 2,
    typeBtn: 'outline',
    label: 'Dự thảo',
    icon: 'pi pi-pencil',
    items: [
      {
        label: 'Văn bản đi',
        icon: 'pi pi-fw pi-plus',
        command: () => {
          openModalDuthao(2);
        }
      },
      {
        label: 'Văn bản nội bộ',
        icon: 'pi pi-fw pi-plus',
        command: () => {
          openModalDuthao(3);
        }
      },
    ]
  },
  {
    id: '6',
    is_order: 3,
    typeBtn: 'outline',
    label: 'Đóng dấu',
    icon: 'pi pi-pencil',
    command: () => {
      openModalStamp(selectedDoc.nav_type);
    }
  },
  {
    id: '3',
    is_order: 4,
    typeBtn: 'outline',
    label: 'Phân phát',
    icon: 'pi pi-send',
    command: () => {
      openModalPublish();
    }
  },
  {
    is_order: 5,
    typeBtn: 'outline',
    label: 'Xử lý',
    icon: 'pi pi-fw pi-check-circle',
    items: [
      {
        id: 12,
        label: 'Trình phê duyệt',
        icon: 'pi pi-arrow-circle-right',
        command: () => {
          openModalApproval();
        }
      },
      {
        id: 13,
        label: 'Duyệt chuyển tiếp',
        icon: 'pi pi-arrow-circle-right',
        command: () => {
          openModalApproval();
        }
      },
      {
        id: 14,
        label: 'Duyệt phát hành',
        icon: 'pi pi-arrow-circle-right',
        command: () => {
          openModalPublishingApproval();
        }
      },
      {
        id: 4,
        label: 'Chuyển đích danh cá nhân',
        icon: 'pi pi-fw pi-user-edit',
        command: () => {
          openModalFollowPerson();
        }
      },
      // {
      //   id: 15,
      //   label: 'Chuyển đến nhóm duyệt (Chọn nhóm)',
      //   icon: 'pi pi-fw pi-users',
      //   command: () => {
      //     openModalFollowGroup();
      //   }
      // },
      {
        id: 15,
        label: 'Chuyển đến phòng ban',
        icon: 'pi pi-fw pi-users',
        command: () => {
          openModalFollowDepartment();
        }
      },
      {
        id: 7,
        label: 'Xác nhận hoàn thành',
        icon: 'pi pi-user-edit',
        command: () => {
          openModalCompleted();
        }
      },
      {
        id: 8,
        label: 'Trả lại',
        icon: 'pi pi-undo',
        command: () => {
          openModalReturn();
        }
      },
      {
        id: 5,
        label: 'Chuyển đóng dấu/vào sổ',
        icon: 'pi pi-arrow-circle-right',
        command: () => {
          openModalTransferStamp();
        }
      }
    ]
  },
  {
    is_order: 6,
    typeBtn: 'outline',
    label: 'Khác',
    icon: '',
    items: [
      {
        id: 10,
        label: 'Copy vào kho dữ liệu',
        icon: 'pi pi-inbox',
        command: () => {
          moveToStore(selectedDoc.value, 1);
        }
      },
      {
        id: 9,
        label: 'Link vào kho dữ liệu',
        icon: 'pi pi-link',
        command: () => {
          moveToStore(selectedDoc.value, 2);
        }
      },
      {
        id: 11,
        label: 'Liên kết công việc',
        icon: 'pi pi-tag',
        command: () => {
          showModalLinkTask();
        }
      },
    ]
  },
  {
    is_order: 7,
    id: 1,
    typeBtn: 'outline',
    label: 'Trục liên thông',
    icon: 'pi pi-external-link',
    is_check_checkbox: true,
    items: [
      {
        id: 'li1',
        label: 'Gửi văn bản',
        icon: 'pi pi-send',
        command: () => {
          openModalSendDocConnect();
        }
      },
    ]
  },
])
const doc_functions = ref([]);
const loadUserFunctions = (docpar) => {
  axios
    .post(
      baseURL
      //baseUrl
      + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_master_receive_functions_list",
        par: [
          { par: "user_key", va: store.getters.user.user_key },
          { par: "doc_master_id", va: docpar ? docpar.doc_master_id : null },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (isFirst.value) isFirst.value = false;
      if (data[0]) {
        doc_functions.value = [];
        var funcs = data[0][0].res;
        if (funcs) {
          let arr = funcs.split(',');
          let clone_functions = defined_doc_functions.value.map(obj => ({ ...obj }));
          clone_functions.forEach(e => {
            if (arr.find(x => x == e.id) || !e.id) {
              doc_functions.value.push(e);
            }
          });
          doc_functions.value.forEach(function (e, idx) {
            if (e.items && e.items.length > 0) {
              e.items_filter = [];
              e.items.forEach(child => {
                if (!child.id || (child.id && arr.find(x => x == child.id))) {
                  e.items_filter.push(child);
                }
              });
              e.items = e.items_filter;
              if (e.items.length === 0) {
                doc_functions.value = doc_functions.value.filter(item => item !== e)
              }
            }
          });
        }
      }
      options.value.loading = false;
    })
    .catch((error) => {
      loadUserFunctions(docpar);
      // toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
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
const checked_docs = ref([]);
const returnCountCheckboxDocs = (count_checkboxs, data_checked) => {
  checked_docs.value = data_checked;
  if(count_checkboxs > 0){
    defined_doc_functions.value.forEach(e => {
        if(e.is_check_checkbox){
          if(!doc_functions.value.find(x=>x.id === e.id))
          doc_functions.value.push(e);
        }
    });
  }
  else{
    doc_functions.value.forEach(e => {
      if(e.is_check_checkbox){
        let idx = doc_functions.value.findIndex(x=>x.id === e.id);
        if(idx > -1){
          doc_functions.splice(idx,1);
        }
      }
    })
  }
};
// Load Category
const category = ref({});
const loadCategorys = () => {
  axios
    .post(
      baseURL
      //baseUrl
      + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_master_category_list",
        par: [
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "user_id", va: store.getters.user.user_id }
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (isFirst.value) isFirst.value = false;
      category.value.issue_places = data[0].filter(x=>!x.is_slider);
      category.value.issue_places_checkbox = data[0].filter(x=>x.is_slider);

      category.value.groups = data[1];
      category.value.org_groups = data[1];

      category.value.org_departments = data[2];
      let obj = renderTreeDV(
        category.value.org_departments,
        "organization_id",
        "organization_name",
        "phòng ban"
      );
      category.value.departments = obj.arrtreeChils;
      
      category.value.urgency = data[3];
      category.value.security = data[4];
      category.value.fields = data[5];
      category.value.send_ways = data[6];
      category.value.receive_places = data[7];

      category.value.dispatch_books = data[8];
      category.value.org_dispatch_books = data[8];

      category.value.positions = data[9];
      category.value.signers = data[10];
      category.value.email = data[11];
      category.value.email_groups = data[12];
      category.value.reservation_numbers = data[13];
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
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
// Load Relative Doc
const loadRelDoc = () => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_master_list_reldoc",
        par: [
          { par: "organization_id", va: options.value.organization_id }
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        allRelateDoc.value = data;
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
}
// list org
const treeorgs = ref({});
const renderTreeDV = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => !data.find(y => y.organization_id === x.parent_id))
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
const loadOrgs = () => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_organization_list",
        par: [{ par: "user_id", va: store.getters.user.user_key }],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        let obj = renderTreeDV(
          data[0],
          "organization_id",
          "organization_name",
          "đơn vị"
        );
        treeorgs.value = obj.arrtreeChils;
        // treeorgs.value = data[0];
      }
    })
    .catch((error) => { });
};
// get dispatch book
// const dispatchBooks = ref([]);
// const getDispatchBook = () => {
//   dispatchBooks.value = category.value.dispatch_books.filter(x => x.nav_type === doc.value.nav_type);
// };
// So hoa
const doc_type_title = ref();
const isViewDoc = ref(false);
const openModalViewDoc = (data) => {
  isViewDoc.value = true;
  let type = data.nav_type;
  let title = type === 1 ? 'đến' : (type === 2 ? 'đi' : 'nội bộ');
  doc_type_title.value = "Văn bản " + title;
  files = [];
  file_one = [];
  listFileUploaded.value = [];
  listFileDel.value = [];
  loadRelDoc();
  if (data != null && data.doc_master_id != null) {
    isAddDoc.value = false;
    headerDoc.value = "Xem văn bản " + title;
    getDetailDocByID(data);
  }
  if(data.is_drafted){
    displayDocDuthao.value = true;
  }
  else{
    displayDocSohoa.value = true;
  }
  submitted.value = false;
}
const changeIsAutoNum = () => {
  let scv = category.value.dispatch_books.find(x => x.dispatch_book_id === doc.value.dispatch_book_id)
  if(doc.value.is_auto_num){
    axios
    .post(
      baseURL
      //baseUrl
      + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_dispatch_book_get_num",
        par: [
          { par: "dispatch_book_id", va: doc.value.dispatch_book_id }
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      scv.current_num = data[0][0].current_num;
      var count_same = 0;
    if(category.value.reservation_numbers.length > 0){
        let auto_num = category.value.reservation_numbers.find(x=>x.num === scv.current_num + count_same + 1 && x.nav_type === doc.value.nav_type);
        while(auto_num){
          count_same++;
          auto_num = category.value.reservation_numbers.find(x=>x.num === scv.current_num + count_same + 1 && x.nav_type === doc.value.nav_type);
        }
    }
    if(doc.value.doc_status_id !== 'dadongdau'){
      doc.value.dispatch_book_num = scv.current_num + count_same + 1;
    }
    else{
      doc.value.dispatch_book_num = scv.current_num ;
    }
    doc.value.dispatch_book_code = doc.value.dispatch_book_num;
        options.value.loading = false;
      })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
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
}
const generateDocCode = () => {
  if(options.value.loading) return false;
  options.value.loading = true;
  axios
    .post(
      baseURL
      //baseUrl
      + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_master_generate_doc_code",
        par: [
          { par: "organization_id", va: options.value.organization_id },
          { par: "department_id", va: (doc.value.department_id && doc.value.is_by_department) ? Object.keys(doc.value.department_id)[0] : null },
          { par: "user_key", va: store.getters.user.user_key },
          { par: "doc_group_id", va: doc.value.doc_group_id },
          { par: "dispatch_book_id", va: doc.value.dispatch_book_id },
          { par: "nav_type", va: doc.value.nav_type },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      let sokh = '';
      for (let i = 0; i < data[0].length; i++) {
        if ((i + 1) === (data[0].length - 1) && !data[0][i + 1].value) {
          data[0][i].separator = '';
        }
          sokh += data[0][i].value + (data[0][i].value ? data[0][i].separator : '');
        }
        auto_doc_code.value = data[0];
        if(doc.value.nav_type === 2 || doc.value.nav_type === 3){
          doc.value.doc_code = sokh;
        }
        options.value.loading = false;
      })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
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
const changeMessageString = (var_exec) => {
  if(!var_exec.message){
    var_exec.is_exported_xml = false;
  }
}
const changeIsByDepartment = () => {
  if(!doc.value.is_by_department){
    if(displayDocSohoa.value)
    doc.value.department_id = null;
  }
  if(doc.value.is_by_department){
    if(displayDocDuthao.value){
      let keyobj = {};
      keyobj[store.getters.user.department_id] = true;
      doc.value.department_id = keyobj;
    }
  }
  if(!displayDocDuthao.value) generateDocCode();
}
// fill noi nhan
const changeReceivePlace = () => {
  doc.value.filter_receive_place = doc.value.receive_place;
}
const fillReceivePlace = (ev) => {
    if(doc.value.filter_receive_place){
      doc.value.filter_receive_place += ', ';
    }
    else{
      doc.value.filter_receive_place = '';
    }
    doc.value.filter_receive_place += ev.value;
    doc.value.receive_place = doc.value.filter_receive_place;
}
// auto fill noi nhan qua mang
const autoFillReceivePlaceOnline = (ev,model,property_name) => {
    if(ev.keyCode === 13){
      let text = model['filter_' + property_name];
      if(text){
      if(!model['arr_' + property_name]) model['arr_' + property_name] = [];
      var search_result_arr = category.value.issue_places.filter(x=>(x.search_code && x.search_code.trim().toLowerCase() == text.trim().toLowerCase()) || x.issue_place_name.trim().toLowerCase() == text.trim().toLowerCase());
      search_result_arr.forEach(function (search_result) {
        if (search_result) {
          if (!search_result.parent_id) {
            search_result.display_name = search_result.display_code;
          }
          else {
            var cur_issue_place = search_result;
            search_result.display_name = search_result.display_code;
            while (cur_issue_place.parent_id) {
              cur_issue_place = category.value.issue_places.find(x => x.issue_place_id === cur_issue_place.parent_id);
              search_result.display_name += '/' + cur_issue_place.display_code;
            }
          }
        }
      })
      if(search_result_arr.length > 1){
        same_receive_place_online.value = search_result_arr;
        showModalReceivePlaceOnline("Chọn nơi nhận qua mạng thích hợp");
        return false;
      }
      else if(search_result_arr.length === 1){
        if(search_result_arr[0].display_name && !model['arr_' + property_name].find(x=>x.value === search_result_arr[0].display_name)){
          model['arr_' + property_name].push({value:search_result_arr[0].display_name});
          fillReceivePlace({value:search_result_arr[0].display_name});
        }
        model['filter_' + property_name] = '';
      }
    } 
    }
   
}
const same_receive_place_online = ref([]);
// Same receive place online selection
const selectReceivePlaceOnline = (receive_place_online) => {
  if(receive_place_online.display_name && !doc.value.arr_receive_place_online.find(x=>x.value === receive_place_online.display_name))
      doc.value.arr_receive_place_online.push({value:receive_place_online.display_name});
      fillReceivePlace({value:receive_place_online.display_name});
}
const headerDialogReceivePlaceOnline = ref();
const displayDialogReceivePlaceOnline = ref(false);
const CloseDiaLogReceivePlaceOnline = () => {
  displayDialogReceivePlaceOnline.value = false;
}
const showModalReceivePlaceOnline = (header_text) => {
  headerDialogReceivePlaceOnline.value = header_text;
  displayDialogReceivePlaceOnline.value = true;
}
// remove receive place online
const removeReceivePlaceOnline = () => {
  if(doc.value.receive_place_online_checkbox.length > 0){
    doc.value.filter_receive_place_online = '';
    doc.value.arr_receive_place_online = [];
  }
}
// auto fill noi ban hanh
const autoFillIssuePlace = (text,model,property_name) => {
    if(text){
      // model[property_name] = '';
      var search_result_arr = category.value.issue_places.filter(x=>(x.search_code && x.search_code.trim().toLowerCase() == text.trim().toLowerCase()) || x.issue_place_name.trim().toLowerCase() == text.trim().toLowerCase());
      search_result_arr.forEach(function (search_result) {
        if (search_result) {
          if (!search_result.parent_id) {
            search_result.display_name = search_result.issue_place_name;
          }
          else {
            var cur_issue_place = search_result;
            search_result.display_name = search_result.issue_place_name;
            while (cur_issue_place.parent_id) {
              cur_issue_place = category.value.issue_places.find(x => x.issue_place_id === cur_issue_place.parent_id);
              search_result.display_name += '/' + cur_issue_place.display_code;
            }
          }
        }
      })
      if(search_result_arr.length > 1){
        same_issue_place.value = search_result_arr;
        showModalIssuePlace("Chọn nơi ban hành thích hợp");
        return false;
      }
      else if(search_result_arr.length === 1){
        model[property_name] = search_result_arr[0].display_name;
      }
    }
}
const headerDialogIssuePlace = ref();
const displayDialogIssuePlace = ref(false);
const CloseDiaLogIssuePlace = () => {
  displayDialogIssuePlace.value = false;
}
const showModalIssuePlace = (header_text) => {
  headerDialogIssuePlace.value = header_text;
  displayDialogIssuePlace.value = true;
}
// change nguoi ky
const changeSigner = (us) => {
  if(doc.value.nav_type === 1){
    doc.value.position = category.value.signers.find(x=>x.signer_name === us.value).position;
  }
  else{
    doc.value.position = all_users.value.find(x=>x.full_name === us.value).position_name;
  }
}
const fileupload = ref(null);
const openModalSohoa = (type, data) => {
  isViewDoc.value = false;
  let title = type === 1 ? 'đến' : (type === 2 ? 'đi' : 'nội bộ');
  doc_type_title.value = "Văn bản " + title;
  if(fileupload.value){
    fileupload.value.clear();
  }
  files = [];
  file_one = [];
  listFileUploaded.value = [];
  listFileDel.value = [];
  loadRelDoc();
  selectedReservationNumber.value = "";
  if (data != null && data.doc_master_id != null) {
    isAddDoc.value = false;
    headerDoc.value = "Cập nhật văn bản " + title;
    getDetailDocByID(data);
  }
  else {
    let is_continued = doc.value.is_continued ? true : false;
    let continued_doc = {};
    if(is_continued){
      continued_doc.dispatch_book_id = doc.value.dispatch_book_id;
      continued_doc.doc_group = doc.value.doc_group;
      continued_doc.doc_group_id = doc.value.doc_group_id;
    }
    isAddDoc.value = true;
    headerDoc.value = "Thêm mới văn bản " + title;
    doc.value = {
      receive_date: new Date(),
      doc_date : new Date(),
      doc_group: category.value.groups.length > 0 ? category.value.groups[0].doc_group_name : null,
      doc_group_id: category.value.groups.length > 0 ? category.value.groups[0].doc_group_id : null,
      organization_id: options.value.organization_id,
      is_auto_num: true,
      is_drafted: false,
      is_inworkflow: false,
      created_by: store.getters.user.user_key,
      nav_type: type,
      rel_docs: [],
      tags: ""
    };
    if(!is_continued) doc.value.dispatch_book_id = category.value.dispatch_books.filter(x => x.nav_type === doc.value.nav_type).length > 0 ? category.value.dispatch_books.filter(x => x.nav_type === doc.value.nav_type)[0].dispatch_book_id : null;
    else{
      doc.value.dispatch_book_id = continued_doc.dispatch_book_id;
      doc.value.doc_group = continued_doc.doc_group;
      doc.value.doc_group_id = continued_doc.doc_group_id;
    }
    if (doc.value.organization_id) {
      let keyobj = {};
      keyobj[doc.value.organization_id] = true;
      doc.value.organization_id = keyobj;
    }
    if (type === 2 || type === 3) {
      doc.value.drafter = store.getters.user.full_name
    };
    if (doc.value.nav_type === 1 && category.value.issue_places && category.value.issue_places.length === 1) {
      doc.value.issue_place = category.value.issue_places[0].issue_place_name;
    }
    else if (doc.value.nav_type !== 1 && treeorgs.value.length === 1) {
      doc.value.issue_place = store.getters.user.organization_name;
    }
    if(!doc.value.dispatch_book_id){
          swal.fire({
          title: "Thông báo!",
          text: "Chưa khởi tạo danh mục khối cơ quan!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return false;
    }
    if(doc.value.is_auto_num){
      changeIsAutoNum();
    }
    generateDocCode();
  }
  displayDocSohoa.value = true;
  submitted.value = false;
};
const isVT = ref(false);
const openModalDuthao = (type, data) => {
  isViewDoc.value = false;
  let title = type === 2 ? 'đi' : 'nội bộ';
  doc_type_title.value = "Văn bản " + title;
  files = [];
  file_one = [];
  listFileUploaded.value = [];
  listFileDel.value = [];
  loadRelDoc();
  if (data != null && data.doc_master_id != null) {
    isAddDoc.value = false;
    headerDoc.value = "Cập nhật văn bản dự thảo " + title;
    getDetailDocByID(data);
  }
  else {
    isAddDoc.value = true;
    headerDoc.value = "Thêm mới văn bản dự thảo " + title;
    doc.value = {
      receive_date: new Date(),
      doc_group: category.value.groups.length > 0 ? category.value.groups[0].doc_group_name : null,
      doc_group_id: category.value.groups.length > 0 ? category.value.groups[0].doc_group_id : null,
      organization_id: options.value.organization_id,
      department_id: options.value.department_id,
      is_auto_num: false,
      is_drafted: true,
      is_inworkflow: false,
      created_by: store.getters.user.user_key,
      nav_type: type,
      drafter: store.getters.user.full_name,
      rel_docs: [],
      tags: "",
      is_by_department: true
    }
    if(store.getters.user.role_id === 'vanthu'){
        isVT.value = true;
    }
    let keyobj = {};
      keyobj[store.getters.user.department_id] = true;
      doc.value.department_id = keyobj;
    if (doc.value.organization_id) {
      let keyobj = {};
      keyobj[doc.value.organization_id] = true;
      doc.value.organization_id = keyobj;
    }
  }
  displayDocDuthao.value = true;
  submitted.value = false;
}
var convertNormalToTreeObj = (model,property_arr) => {
  property_arr.forEach(function(prop){
    if (model[prop]) {
      let keyobj = {};
      keyobj[model[prop]] = true;
      model[prop] = keyobj;
    }
  })
};
// Luu va tiep tuc
const saveDocContinued = (isFormValid) => {
  doc.value.is_continued = true;
  saveDoc(isFormValid);
}
const saveDoc = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (!doc.value.is_reservation_number && category.value.reservation_numbers.length > 0 
  && category.value.reservation_numbers.find(x=>x.nav_type === doc.value.nav_type && x.reservation_code === doc.value.doc_code)) {
    swal.fire({
      title: "Thông báo!",
      text: "Số đã được giữ! Không thể chọn",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (doc.value.compendium.length >= 500) {
    swal.fire({
      title: "Thông báo!",
      text: "Trích yếu nội dung không được vượt quá 500 kí tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (doc.value.is_by_department && !doc.value.department_id) {
    swal.fire({
      title: "Thông báo!",
      text: "Chưa chọn phòng ban!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (typeof doc.value.doc_date == "string") {
    let startDay = doc.value.doc_date.split("/");
    doc.value.doc_date = new Date(
      startDay[2] + "/" + startDay[1] + "/" + startDay[0]
    );
  }
  if (typeof doc.value.receive_date == "string") {
    let endDay = doc.value.receive_date.split("/");
    doc.value.receive_date = new Date(
      endDay[2] + "/" + endDay[1] + "/" + endDay[0]
    );
  }
  if (typeof doc.value.deadline_date == "string") {
    let endDay = doc.value.deadline_date.split("/");
    doc.value.deadline_date = new Date(
      endDay[2] + "/" + endDay[1] + "/" + endDay[0]
    );
  }
  if (typeof doc.value.effective_date == "string") {
    let endDay = doc.value.effective_date.split("/");
    doc.value.effective_date = new Date(
      endDay[2] + "/" + endDay[1] + "/" + endDay[0]
    );
  }
  if (typeof doc.value.expiration_date == "string") {
    let endDay = doc.value.expiration_date.split("/");
    doc.value.expiration_date = new Date(
      endDay[2] + "/" + endDay[1] + "/" + endDay[0]
    );
  }
  if (moment(doc.value.doc_date).isAfter(new Date(), 'day')) {
    swal.fire({
      title: "Thông báo!",
      text: "Ngày văn bản không được vượt quá ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (moment(doc.value.receive_date).isAfter(new Date(), 'day')) {
    swal.fire({
      title: "Thông báo!",
      text: "Ngày đến không được vượt quá ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (doc.value.nav_type === 1 && moment(doc.value.doc_date).isAfter(doc.value.receive_date, 'day')) {
    swal.fire({
      title: "Thông báo!",
      text: "Ngày đến không được nhỏ hơn ngày văn bản!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (moment(doc.value.effective_date).isAfter(doc.value.expiration_date, 'day')) {
    swal.fire({
      title: "Thông báo!",
      text: "Ngày có hiệu lực không được lớn hơn ngày hết hiệu lực!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (doc.value.hold_time && moment(new Date()).isAfter(doc.value.hold_time)) {
    swal.fire({
      title: "Thông báo!",
      text: "Thời gian tổ chức không được trước thời điểm hiện tại",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if(!doc.value.doc_group_id) doc.value.doc_group = null;
  // end valid date
  let formData = new FormData();
  if (file_one.length == 0 && RequiredFileUploaded.length == 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Chọn file văn bản số hoá trước khi lưu!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  for (var i = 0; i < file_one.length; i++) {
    let file = file_one[i];
    formData.append("doc_file", file);
  }
  if (file_one.length > 0 && !isAddDoc.value) {
    deleteDocMainFile();
  }
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  if (doc.value.key_tags != null) {
    doc.value.tags = doc.value.key_tags.toString();
  }
  if (doc.value.key_fields != null && doc.value.key_fields.length > 0) {
    doc.value.field_name = "";
    doc.value.key_fields.forEach((element, i) => {
      if (doc.value.field_name != "") {
        doc.value.field_name += ',';
      }
      doc.value.field_name += element.field_name;
    });
  }
  else {
    doc.value.field_name = null;
  }

  let listDocRelate = [];
  if (doc.value.relate_doc != null && doc.value.relate_doc != "") {
    if (doc.value.relate_doc.length > 0) {
      doc.value.relate_doc.forEach((element, i) => {
        let relateDoc = { doc_master_id: element.doc_master_id, compendium: element.compendium };
        listDocRelate.push(relateDoc);
      });
    }
  }
  if (doc.value.organization_id) {
    doc.value.organization_id = Object.keys(doc.value.organization_id)[0];
  }
  if (doc.value.department_id) {
    doc.value.department_id = Object.keys(doc.value.department_id)[0];
  }
  if (doc.value.composing_unit_id) {
    doc.value.composing_unit_id = Object.keys(doc.value.composing_unit_id)[0];
    doc.value.composing_unit = category.value.org_departments.find(x=>x.department_id === doc.value.composing_unit_id)?.department_name;
  }
  if (doc.value.arr_receive_place_online != null && doc.value.arr_receive_place_online.length > 0) {
    doc.value.receive_place_online = "";
    doc.value.arr_receive_place_online.map(x=>x.value).forEach((element, i) => {
      if (doc.value.receive_place_online != "") {
        doc.value.receive_place_online += ',';
      }
      doc.value.receive_place_online += element;
    });
  }
  else {
    doc.value.receive_place_online = null;
  }
  if (doc.value.receive_place_online_checkbox != null && doc.value.receive_place_online_checkbox.length > 0) {
    doc.value.receive_place_online_check = "";
    doc.value.receive_place_online_checkbox.forEach((element, i) => {
      if (doc.value.receive_place_online_check != "") {
        doc.value.receive_place_online_check += ',';
      }
      doc.value.receive_place_online_check += element;
    });
  }
  else {
    doc.value.receive_place_online_check = null;
  }
  
  formData.append("doc", JSON.stringify(doc.value));
  formData.append("doc_relates", JSON.stringify(listDocRelate));
  formData.append("auto_doc_code", JSON.stringify(auto_doc_code.value));
  formData.append("department_id", JSON.stringify(store.getters.user.department_id));
  formData.append("is_reservation_number", doc.value.is_reservation_number ? true : false);
  if (!isAddDoc.value) {
    listFileDel.value = listFileDel.value.filter(x => listFileUploaded.value.filter(y => y.file_id == x.file_id).length == 0);
    formData.append("fileUploadOld", JSON.stringify(listFileDel.value));
  };
  if (doc.value.file_path === null) {
    formData.append("docUploadOld", del_docfilepath.value);
  };
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/Add_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        if(!response.data.notunique_case){
          swal.close();
          toast.success("Cập nhật văn bản thành công!");
          if(!doc.value.is_continued){
            closeDialog('sohoa', 'reload');
          }
          else{
            reloadDoc();
            openModalSohoa(doc.value.nav_type);
          }
        }
        else if(response.data.notunique_case === 1){
          swal.fire({
                        icon: 'warning',
                        type: 'warning',
                        title: '',
                        text: 'Số vào khối cơ quan đã tồn tại ! Hệ thống tự động tăng lên số hợp lệ gần nhất !'
                    }).then((result) => {
                        loadCategorys();

                        toast.success("Cập nhật văn bản thành công!");
                        closeDialog('sohoa', 'reload');
                    });
        }
      } 
      else {
        let msg = "";
        switch (response.data.notunique_case) {
          case "2":
            msg = "Số vào sổ đang bị trùng với văn bản khác! Vui lòng nhập lại!";
            break;
          case "3":
            msg = "Số ký hiệu trùng! Vui lòng nhập số khác!";
            break;
          default:
            msg = "Xảy ra lỗi khi cập nhật văn bản.";
            break;
        }
        swal.fire({
          title: "Thông báo",
          text: msg,
          icon: "error",
          confirmButtonText: "OK",
        });
        convertNormalToTreeObj(doc.value,['organization_id','department_id','composing_unit_id']);
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
      convertNormalToTreeObj(doc.value,['organization_id','department_id','composing_unit_id']);
    });
}
const saveDocDuthao = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if(!doc.value.doc_group_id) doc.value.doc_group = null;
  let formData = new FormData();
  if (file_one.length == 0 && !doc.value.file_path) {
    swal.fire({
      title: "Thông báo!",
      text: "Chọn file văn bản đính kèm trước khi lưu!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  for (var i = 0; i < file_one.length; i++) {
    let file = file_one[i];
    formData.append("doc_file", file);
  }
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  if (doc.value.key_tags != null) {
    doc.value.tags = doc.value.key_tags.toString();
  }
  
  if (doc.value.key_fields != null && doc.value.key_fields.length > 0) {
    doc.value.field_name = "";
    doc.value.key_fields.forEach((element, i) => {
      if (doc.value.field_name != "") {
        doc.value.field_name += ',';
      }
      doc.value.field_name += element.field_name;
    });
  }
  else {
    doc.value.field_name = null;
  }
  let listDocRelate = [];
  if (doc.value.relate_doc != null && doc.value.relate_doc != "") {
    if (doc.value.relate_doc.length > 0) {
      doc.value.relate_doc.forEach((element, i) => {
        let relateDoc = { doc_master_id: element.doc_master_id, compendium: element.compendium };
        listDocRelate.push(relateDoc);
      });
    }
  }
  if (doc.value.organization_id) {
    doc.value.organization_id = Object.keys(doc.value.organization_id)[0];
  }
  if (doc.value.department_id) {
    doc.value.department_id = Object.keys(doc.value.department_id)[0];
  }
  formData.append("doc", JSON.stringify(doc.value));
  formData.append("doc_relates", JSON.stringify(listDocRelate));
  formData.append("department_id", JSON.stringify(store.getters.user.department_id));
  if (!isAddDoc.value) {
    listFileDel.value = listFileDel.value.filter(x => listFileUploaded.value.filter(y => y.file_id == x.file_id).length == 0);
    formData.append("fileUploadOld", JSON.stringify(listFileDel.value));
  };
  if (doc.value.file_path === null) {
    formData.append("docUploadOld", del_docfilepath.value);
  };

  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/Add_DocDuthao`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật văn bản thành công!");
        closeDialog('duthao', 'reload');
      } else {
        console.log(response.data.ms);
        swal.fire({
          title: "Thông báo",
          text: "Xảy ra lỗi khi cập nhật văn bản.",
          icon: "error",
          confirmButtonText: "OK",
        });
        convertNormalToTreeObj(doc.value,['organization_id','department_id']);
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
      convertNormalToTreeObj(doc.value,['organization_id','department_id']);
    });
}
const checkUpdateViewed = (docobj) => {
    if(!docobj.view_id || !docobj.view_date){
      emitter.emit("emitData", { type: "updateViewDoc", data: null });
    }
}
const closeDialog = (type_doc, type) => {
  if(['sohoa', 'duthao', 'laysogiu'].indexOf(type_doc) === -1){
    checkUpdateViewed(selectedDoc.value);
  }
  switch (type_doc) {
    case 'sohoa':
      displayDocSohoa.value = false;
      doc.value = {
        receive_date: new Date(),
        doc_date: doc.value.nav_type !== 1 ? new Date() : null,
        doc_group: category.value.groups.length > 0 ? category.value.groups[0].doc_group_name : null,
        organization_id: options.value.organization_id,
        is_auto_num: true,
        is_drafted: false,
        is_inworkflow: false,
        created_by: store.getters.user.user_key,
        nav_type: doc.value.nav_type,
        urgency: category.value.urgency.length > 0 ? category.value.urgency[0].urgency_name : null,
        rel_docs: [],
        tags: ""
      };
      break;
    case 'duthao':
      displayDocDuthao.value = false;
      break;
    case 'phanphat':
      displayPublish.value = false;
      break;
    case 'duyetcanhan':
      displayFollowPerson.value = false;
      break;
      case 'duyetnhom':
      displayFollowGroup.value = false;
      break;
      case 'duyetphongban':
      displayFollowDepartment.value = false;
      break;
    case 'chuyendongdau':
      displayTransferStamp.value = false;
      break;
    case 'dongdau':
      displayStamp.value = false;
      doc.value = {
        compendium: null,
        notes: "",
      }
      break;
    case 'xacnhanhoanthanh':
      displayCompleted.value = false;
      break;
      case 'tralai':
      displayReturn.value = false;
      break;
      case 'pheduyet':
      displayApproval.value = false;
      break;
      case 'laysogiu':
      displayReservationNumber.value = false;
      break;
  }
  if (type != null) {
    reloadDoc();
  }
};
const onRefresh = () => {
  reloadDoc(true);
}
const reloadDoc = (isRefresh) => {
  emitter.emit("emitData", { type: "reloadDoc", data: isRefresh ? 'refresh' : null });
}
// Edit
const goToDetailSohoa = (docpar) => {
  openModalSohoa(docpar.nav_type, docpar);
};
const goToDetailDuthao = (docpar) => {
  openModalDuthao(docpar.nav_type, docpar);
};
const getDetailDocByID = (docpar) => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_master_get_byID",
        par: [
          { par: "doc_master_id", va: docpar.doc_master_id },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      let objDoc = data[0][0];
      convertDatefromDB(objDoc);
      if (objDoc.organization_id) {
        let keyobj = {};
        keyobj[objDoc.organization_id] = true;
        objDoc.organization_id = keyobj;
      }
      if (objDoc.department_id) {
        let keyobj = {};
        keyobj[objDoc.department_id] = true;
        objDoc.department_id = keyobj;
      }
      if (objDoc.composing_unit_id) {
        let keyobj = {};
        keyobj[objDoc.composing_unit_id] = true;
        objDoc.composing_unit_id = keyobj;
      }
      if (objDoc.tags != null && objDoc.tags.length > 1) {
        if (!Array.isArray(objDoc.tags)) {
          objDoc.key_tags = objDoc.tags.split(",");
        }
      }
      if (objDoc.field_name != null && objDoc.field_name.length > 1) {
        if (!Array.isArray(objDoc.field_name)) {
          let listFields = objDoc.field_name.split(",");
          objDoc.key_fields = [];
          listFields.forEach((element, i) => {
            let field = category.value.fields.find(x => x.field_name.trim().toUpperCase() === element.trim().toUpperCase());
            objDoc.key_fields.push(field);
          });
        }
      }
      if (objDoc.receive_place_online != null && objDoc.receive_place_online.length > 1) {
        if (!Array.isArray(objDoc.receive_place_online)) {
          let listReceivePlaceOnline = objDoc.receive_place_online.split(",");
          objDoc.arr_receive_place_online = [];
          listReceivePlaceOnline.forEach((element, i) => {
            objDoc.arr_receive_place_online.push({value: element});
          });
        }
      }
      if (objDoc.receive_place_online_check != null && objDoc.receive_place_online_check.length > 1) {
        if (!Array.isArray(objDoc.receive_place_check)) {
          let listReceivePlaceOnlineCheck = objDoc.receive_place_online_check.split(",");
          objDoc.receive_place_online_checkbox = [];
          listReceivePlaceOnlineCheck.forEach((element, i) => {
            objDoc.receive_place_online_checkbox.push(element);
          });
        }
      }
      data[1].forEach((element, i) => {
        element.file_type = element.file_type.toLowerCase();
      });
      listFileUploaded.value = data[1];
      listFileDel.value = data[1];
      objDoc.relate_doc = data[2];
      doc.value = objDoc;
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
// Delete 
const doc_del = ref({});
const delDoc = () => {
  let docpar = doc_del.value;
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá văn bản này không!",
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
          .delete(baseUrlCheck + "/api/DocMain/Delete_Doc", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [docpar.doc_master_id],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá văn bản thành công!");
              options.value.PageNo = 0;
              reloadDoc();
            } else {
              console.log(response.data.ms);
              swal.fire({
                title: "Thông báo",
                text: "Xảy ra lỗi khi xóa văn bản",
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
// doc_code_auto gen
const auto_doc_code = ref();
// Relate doc
const allRelateDoc = ref([]);
const resetRelate = () => {
  doc.value.relate_doc = [];
};
// Tags
const filterDocTags = ref();
// Fields
const filterDocField = ref();
// Doc group
const changeDocGroup = () => {
  doc.value.doc_group = category.value.groups.find(x => x.doc_group_id === doc.value.doc_group_id).doc_group_name;
}
// -----------------Publish-----------------
// category
const publish_groups = ref([]);
const seetoknow_groups = ref([]);
const track_groups = ref([]);
const loadDocRoleGroups = () => {
  axios
    .post(
      baseURL
      //baseUrl
      + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_list_role_groups",
        par: [
          { par: "organization_id", va: options.value.organization_id }
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (isFirst.value) isFirst.value = false;
      if (data[0].length > 0) {
        data[0].forEach(function (it) {
          if (it.users) {
            it.users = JSON.parse(it.users);
          }
        })
        publish_groups.value = data[0].filter(x => x.is_type === 0 || x.is_type === 2);
        track_groups.value = data[0];
        seetoknow_groups.value = data[0].filter(x => x.is_type === 1 || x.is_type === 2);
        follow_groups.value = data[0].filter(x => x.is_type === 3 && ((x.users && x.users.length > 0 && !x.is_bydepartment) || x.is_bydepartment));
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
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
// main
const displayPublish = ref(false);
const headerPublish = ref();
const publish_item = ref({});
const openModalPublish = () => {
  // if(!selectedDoc.value.file_path){
  //   swal.fire({
  //         title: "Thông báo",
  //         text: "Chưa có văn bản đính kèm!",
  //         icon: "error",
  //         confirmButtonText: "OK",
  //       });
  //   return false;
  // }
  headerPublish.value = "Phân phát văn bản"
  displayPublish.value = true;
  publish_item.value = {
    created_date: new Date(),
    created_date_str: moment(new Date()).format("DD/MM/YYYY"),
    follow_id: selectedDoc.value.follow_id,
    doc_master_id: selectedDoc.value.doc_master_id,
    organization_id: options.value.organization_id,
    organizations: [],
    groups: [],
    users: [],
    departments: {}
  }
}
const savePublishDoc = () => {
  submitted.value = true;
  if (publish_item.value.organizations.length === 0 && publish_item.value.groups.length === 0 && publish_item.value.users.length === 0 && Object.keys(publish_item.value.departments).length === 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Chưa chọn đối tượng nhận phân phát!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  publish_item.value.org_ids = [];
  for (var key in publish_item.value.organizations) {
    publish_item.value.org_ids.push(key);
  }
  let dep_ids = [];
  if(Object.keys(publish_item.value.departments).length > 0){
    for(var key in publish_item.value.departments){
    if(publish_item.value.departments[key].checked){
      dep_ids.push(key);
    }
  }
  }
  let formData = new FormData();
  formData.append("publish_item", JSON.stringify(publish_item.value));
  formData.append("orgs", JSON.stringify(publish_item.value.org_ids));
  formData.append("groups", JSON.stringify(publish_item.value.groups));
  formData.append("users", JSON.stringify(publish_item.value.users));
  formData.append("departments", JSON.stringify(dep_ids));
  formData.append("is_exported_xml", publish_item.value.is_exported_xml ? true : false);
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/Publish_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật văn bản thành công!");
        if (isAddDoc.value) {
          closeDialog('phanphat', 'reload');
        }
        else {
          closeDialog('phanphat');
          // loadDataLaw(true);
          // showDetailLaw(detailLaw.value);
        }
      } else {
        console.log(response.data.ms);
        swal.fire({
          title: "Thông báo",
          text: "Xảy ra lỗi khi phân phát văn bản.",
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
//----------------Duyet ca nhan-------------
const displayFollowPerson = ref(false);
const headerFollowPerson = ref();
const followperson_item = ref({});
const filterFollowPerson = (type) => {
  switch (type) {
    case 1:
      followperson_item.value.trackers = followperson_item.value.trackers.filter(
        (a) =>
          followperson_item.value.hosts.findIndex(

            (b) => b === a
          ) === -1
      );
      followperson_item.value.seetoknow_users = followperson_item.value.seetoknow_users.filter(
        (a) =>
          followperson_item.value.hosts.findIndex(

            (b) => b === a
          ) === -1
      );
      followperson_item.value.track_groups.forEach(function (gr, index) {
        let track_groups_ids = (track_groups.value.find(x => x.role_group_id === gr)).users.map(x => x.user_id);
        if (followperson_item.value.hosts.find(x => track_groups_ids.includes(x))) {
          followperson_item.value.track_groups.splice(index, 1);
        }
      })
      followperson_item.value.seetoknow_groups.forEach(function (gr, index) {
        let seetoknow_groups_ids = (seetoknow_groups.value.find(x => x.role_group_id === gr)).users.map(x => x.user_id);
        if (followperson_item.value.hosts.find(x => seetoknow_groups_ids.includes(x))) {
          followperson_item.value.seetoknow_groups.splice(index, 1);
        }
      })
      for(var key in followperson_item.value.track_departments) {
        let dep = category.value.org_departments.find(x=>x.organization_id == key)
          if(dep){
            if (followperson_item.value.hosts.find(x => dep.user_id === x)) {
              delete followperson_item.value.track_departments[key];
            }
          }
      };
      break;
    case 2:
      followperson_item.value.hosts = followperson_item.value.hosts.filter(
        (a) =>
          followperson_item.value.trackers.findIndex(

            (b) => b === a
          ) === -1
      );
      followperson_item.value.seetoknow_users = followperson_item.value.seetoknow_users.filter(
        (a) =>
          followperson_item.value.trackers.findIndex(

            (b) => b === a
          ) === -1
      );
      followperson_item.value.track_groups.forEach(function (gr, index) {
        let track_groups_ids = (track_groups.value.find(x => x.role_group_id === gr)).users.map(x => x.user_id);
        if (followperson_item.value.trackers.find(x => track_groups_ids.includes(x))) {
          followperson_item.value.track_groups.splice(index, 1);
        }
      })
      followperson_item.value.seetoknow_groups.forEach(function (gr, index) {
        let seetoknow_groups_ids = (seetoknow_groups.value.find(x => x.role_group_id === gr)).users.map(x => x.user_id);
        if (followperson_item.value.trackers.find(x => seetoknow_groups_ids.includes(x))) {
          followperson_item.value.seetoknow_groups.splice(index, 1);
        }
      })
      for(var key in followperson_item.value.track_departments) {
        let dep = category.value.org_departments.find(x=>x.organization_id == key)
          if(dep){
            if (followperson_item.value.trackers.find(x => dep.user_id === x)) {
              delete followperson_item.value.track_departments[key];
            }
          }
      };
      break;
    case 3:
      followperson_item.value.track_groups.forEach(function (gr, index) {
        let us = track_groups.value.find(x => x.role_group_id === gr).users;
        followperson_item.value.hosts = followperson_item.value.hosts.filter(
          (a) =>
            us.findIndex(

              (b) => b.user_id === a
            ) === -1
        );
        followperson_item.value.trackers = followperson_item.value.trackers.filter(
          (a) =>
            us.findIndex(

              (b) => b.user_id === a
            ) === -1
        );
        followperson_item.value.seetoknow_users = followperson_item.value.seetoknow_users.filter(
          (a) =>
            us.findIndex(

              (b) => b.user_id === a
            ) === -1
        );
        followperson_item.value.seetoknow_groups.forEach(function (gr_inside, index) {
          let seetoknow_groups_ids = (seetoknow_groups.value.find(x => x.role_group_id === gr_inside)).users.map(x => x.user_id);
          if ((seetoknow_groups.value.find(x => x.role_group_id === gr_inside)).users.find(x => seetoknow_groups_ids.includes(x.user_id))) {
            followperson_item.value.seetoknow_groups.splice(index, 1);
          }
        })
        for(var key in followperson_item.value.track_departments) {
        let dep = category.value.org_departments.find(x=>x.organization_id == key)
          if(dep){
            if (us.find(x => dep.user_id === x.user_id)) {
              delete followperson_item.value.track_departments[key];
            }
          }
      };
      })
      break;
    case 4:
      followperson_item.value.hosts = followperson_item.value.hosts.filter(
        (a) =>
          followperson_item.value.seetoknow_users.findIndex(

            (b) => b === a
          ) === -1
      );
      followperson_item.value.trackers = followperson_item.value.trackers.filter(
        (a) =>
          followperson_item.value.seetoknow_users.findIndex(

            (b) => b === a
          ) === -1
      );
      followperson_item.value.track_groups.forEach(function (gr, index) {
        let track_groups_ids = (track_groups.value.find(x => x.role_group_id === gr)).users.map(x => x.user_id);
        if (followperson_item.value.seetoknow_users.find(x => track_groups_ids.includes(x))) {
          followperson_item.value.track_groups.splice(index, 1);
        }
      })
      followperson_item.value.seetoknow_groups.forEach(function (gr, index) {
        let seetoknow_groups_ids = (seetoknow_groups.value.find(x => x.role_group_id === gr)).users.map(x => x.user_id);
        if (followperson_item.value.seetoknow_users.find(x => seetoknow_groups_ids.includes(x))) {
          followperson_item.value.seetoknow_groups.splice(index, 1);
        }
      })
      for(var key in followperson_item.value.track_departments) {
        let dep = category.value.org_departments.find(x=>x.organization_id == key)
          if(dep){
            if (followperson_item.value.seetoknow_users.find(x => dep.user_id === x)) {
              delete followperson_item.value.track_departments[key];
            }
          }
      };
      break;
    case 5:
      followperson_item.value.seetoknow_groups.forEach(function (gr, index) {
        let us = seetoknow_groups.value.find(x => x.role_group_id === gr).users;
        followperson_item.value.hosts = followperson_item.value.hosts.filter(
          (a) =>
            us.findIndex(

              (b) => b.user_id === a
            ) === -1
        );
        followperson_item.value.seetoknow_users = followperson_item.value.seetoknow_users.filter(
          (a) =>
            us.findIndex(

              (b) => b.user_id === a
            ) === -1
        );
        followperson_item.value.trackers = followperson_item.value.trackers.filter(
          (a) =>
            us.findIndex(

              (b) => b.user_id === a
            ) === -1
        );
        followperson_item.value.track_groups.forEach(function (gr, index) {
          let track_groups_ids = (track_groups.value.find(x => x.role_group_id === gr)).users.map(x => x.user_id);
          if ((track_groups.value.find(x => x.role_group_id === gr)).users.find(x => track_groups_ids.includes(x.user_id))) {
            followperson_item.value.track_groups.splice(index, 1);
          }
        })
        for(var key in followperson_item.value.track_departments) {
          let dep = category.value.org_departments.find(x=>x.organization_id == key)
            if(dep){
              if (us.find(x => dep.user_id === x.user_id)) {
                delete followperson_item.value.track_departments[key];
              }
          }
        };
      })
      break;
    case 6:
      for (var key in followperson_item.value.track_departments) {
        let dep = category.value.org_departments.find(x => x.organization_id == key)
        if (dep) {
          followperson_item.value.hosts = followperson_item.value.hosts.filter(
            (a) =>
              a !== dep.user_id
          );
          followperson_item.value.trackers = followperson_item.value.trackers.filter(
            (a) =>
            a !== dep.user_id
          );
          followperson_item.value.track_groups.forEach(function (gr, index) {
            let track_groups_ids = (track_groups.value.find(x => x.role_group_id === gr)).users.map(x => x.user_id);
            if (track_groups_ids.includes(dep.user_id)) {
              followperson_item.value.track_groups.splice(index, 1);
            }
          })
          followperson_item.value.seetoknow_users = followperson_item.value.seetoknow_users.filter(
            (a) =>
            a !== dep.user_id
          );
          followperson_item.value.seetoknow_groups.forEach(function (gr, index) {
            let seetoknow_groups_ids = (seetoknow_groups.value.find(x => x.role_group_id === gr)).users.map(x => x.user_id);
            if (seetoknow_groups_ids.includes(dep.user_id)) {
              followperson_item.value.seetoknow_groups.splice(index, 1);
            }
          })
        }
      };
      break;
    default:
      break;
  }
}
const openModalFollowPerson = () => {
  // if(!selectedDoc.value.file_path){
  //   swal.fire({
  //         title: "Thông báo",
  //         text: "Chưa có văn bản đính kèm!",
  //         icon: "error",
  //         confirmButtonText: "OK",
  //       });
  //   return false;
  // }
  headerFollowPerson.value = "Chuyển văn bản";
  displayFollowPerson.value = true;
  files = [];
  listFileUploaded.value = [];
  followperson_item.value = {
    created_date: new Date(),
    created_date_str: moment(new Date()).format("DD/MM/YYYY"),
    follow_id: selectedDoc.value.follow_id,
    doc_master_id: selectedDoc.value.doc_master_id,
    organization_id: options.value.organization_id,
    is_deadline: false,
    hosts: [],
    trackers: [],
    track_groups: [],
    seetoknow_users: [],
    seetoknow_groups: [],
    track_departments: []
  };
}
const saveFollowPerson = () => {
  submitted.value = true;
  if (followperson_item.value.hosts.length === 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Hãy xác định người chủ trì!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  let track_dep_ids = [];
  if(Object.keys(followperson_item.value.track_departments).length > 0){
    for(var key in followperson_item.value.track_departments){
    if(followperson_item.value.track_departments[key].checked){
      track_dep_ids.push(key);
    }
  }
}
  if (followperson_item.value.is_deadline && moment(new Date()).isAfter(followperson_item.value.deadline_date, 'day')) {
    swal.fire({
      title: "Thông báo!",
      text: "Hạn xử lý không thể nhỏ hơn ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (typeof followperson_item.value.deadline_date == "string") {
    let startDay = followperson_item.value.deadline_date.split("/");
    followperson_item.value.deadline_date = new Date(
      startDay[2] + "/" + startDay[1] + "/" + startDay[0]
    );
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  // Chu tri mot nguoi trong truong hop van ban du thao
  if(selectedDoc.value.is_drafted){
    followperson_item.value.hosts = [followperson_item.value.hosts];
  }
  //
  formData.append("followperson_item", JSON.stringify(followperson_item.value));
  formData.append("hosts", JSON.stringify(followperson_item.value.hosts));
  formData.append("trackers", JSON.stringify(followperson_item.value.trackers));
  formData.append("track_groups", JSON.stringify(followperson_item.value.track_groups));
  formData.append("seetoknow_users", JSON.stringify(followperson_item.value.seetoknow_users));
  formData.append("seetoknow_groups", JSON.stringify(followperson_item.value.seetoknow_groups));
  formData.append("track_departments", JSON.stringify(track_dep_ids));
  formData.append("is_exported_xml", followperson_item.value.is_exported_xml ? true : false);
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/FollowPerson_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật văn bản thành công!");
        closeDialog('duyetcanhan', 'reload');
      } else {
        console.log(response.data.ms);
        swal.fire({
          title: "Thông báo",
          text: "Xảy ra lỗi khi duyệt cá nhân văn bản.",
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
//----------------Duyet nhom-----------------
// category
const follow_groups = ref([]);
//
const displayFollowGroup = ref(false);
const headerFollowGroup = ref();
const followgroup_item = ref({});
const filterFollowGroup = (type) => {
  switch (type) {
    case 1:
    let fl_gr = follow_groups.value.find(x => x.role_group_id === followgroup_item.value.receive_by);
    if(fl_gr.users && fl_gr.users.length > 0){
      let us = fl_gr.users;
      followgroup_item.value.trackers = followgroup_item.value.trackers.filter(
          (a) =>
            us.findIndex(
              (b) => b.user_id === a
            ) === -1
        );
    }
      break;
    case 2:
      let fl_gr_us = follow_groups.value.find(x => x.role_group_id === followgroup_item.value.receive_by);
      if(fl_gr_us.users && fl_gr_us.users.length > 0){
        let fl_gr_us_ids = fl_gr_us.users.map(x => x.user_id);
        if (followgroup_item.value.trackers.find(x => fl_gr_us_ids.includes(x))) {
          followgroup_item.value.receive_by = null;
        }
      }
      break;
    default:
      break;
  }
}
const openModalFollowGroup = () => {
  // if(!selectedDoc.value.file_path){
  //   swal.fire({
  //         title: "Thông báo",
  //         text: "Chưa có văn bản đính kèm!",
  //         icon: "error",
  //         confirmButtonText: "OK",
  //       });
  //   return false;
  // }
  headerFollowGroup.value = "Chuyển văn bản";
  displayFollowGroup.value = true;
  files = [];
  listFileUploaded.value = [];
  followgroup_item.value = {
    created_date: new Date(),
    created_date_str: moment(new Date()).format("DD/MM/YYYY"),
    follow_id: selectedDoc.value.follow_id,
    doc_master_id: selectedDoc.value.doc_master_id,
    organization_id: options.value.organization_id,
    is_deadline: false,
    trackers: []
  };
}
const saveFollowGroup = () => {
  submitted.value = true;
  if (!followgroup_item.value.receive_by) {
    swal.fire({
      title: "Thông báo!",
      text: "Hãy xác định nhóm nhận!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (followgroup_item.value.is_deadline && moment(new Date()).isAfter(followgroup_item.value.deadline_date, 'day')) {
    swal.fire({
      title: "Thông báo!",
      text: "Hạn xử lý không thể nhỏ hơn ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (typeof followgroup_item.value.deadline_date == "string") {
    let startDay = followgroup_item.value.deadline_date.split("/");
    followgroup_item.value.deadline_date = new Date(
      startDay[2] + "/" + startDay[1] + "/" + startDay[0]
    );
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  formData.append("followgroup_item", JSON.stringify(followgroup_item.value));
  formData.append("trackers", JSON.stringify(followgroup_item.value.trackers));
  formData.append("is_exported_xml", followgroup_item.value.is_exported_xml ? true : false);
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/FollowGroup_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật văn bản thành công!");
        closeDialog('duyetnhom', 'reload');
      } else {
        swal.fire({
          title: "Thông báo",
          text: response.data.ms ? response.data.ms : "Xảy ra lỗi khi duyệt nhóm văn bản.",
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
//----------------Duyet theo phong ban-----------------
const displayFollowDepartment = ref(false);
const headerFollowDepartment = ref();
const followdepartment_item = ref({});
const filterFollowDepartment = (type) => {
  switch (type) {
    case 1:
    for(var key in followdepartment_item.value.main_departments) {
        if(followdepartment_item.value.main_departments[key].checked){
          if(followdepartment_item.value.track_departments[key] && followdepartment_item.value.track_departments[key].checked){
            delete followdepartment_item.value.track_departments[key];
          }
          let dep = category.value.org_departments.find(x=>x.organization_id == key)
          if(dep){
            followdepartment_item.value.trackers = followdepartment_item.value.trackers.filter(
              (a) =>
              a !== dep.user_id
            );
          }
        }
    };
      break;
    case 2:
    for(var key in followdepartment_item.value.track_departments) {
        if(followdepartment_item.value.track_departments[key].checked){
          if(followdepartment_item.value.main_departments[key] && followdepartment_item.value.main_departments[key].checked){
            delete followdepartment_item.value.main_departments[key];
          }
          let dep = category.value.org_departments.find(x=>x.organization_id == key)
          if(dep){
            followdepartment_item.value.trackers = followdepartment_item.value.trackers.filter(
              (a) =>
              a !== dep.user_id
            );
          }
        }
    };
      break;
      case 3:
    for(var key in followdepartment_item.value.track_departments) {
        if(followdepartment_item.value.track_departments[key] && followdepartment_item.value.track_departments[key].checked){
          let dep = category.value.org_departments.find(x=>x.organization_id == key)
          if(followdepartment_item.value.trackers.find(x=>x === dep.user_id)){
            delete followdepartment_item.value.track_departments[key];
          }
        }
    };
    for(var key in followdepartment_item.value.main_departments) {
        if(followdepartment_item.value.main_departments[key] && followdepartment_item.value.main_departments[key].checked){
          let dep = category.value.org_departments.find(x=>x.organization_id == key)
          if(followdepartment_item.value.trackers.find(x=>x === dep.user_id)){
            delete followdepartment_item.value.main_departments[key];
          }
        }
    };
      break;
    default:
      break;
  }
}
const openModalFollowDepartment = () => {
  // if(!selectedDoc.value.file_path){
  //   swal.fire({
  //         title: "Thông báo",
  //         text: "Chưa có văn bản đính kèm!",
  //         icon: "error",
  //         confirmButtonText: "OK",
  //       });
  //   return false;
  // }
  headerFollowDepartment.value = "Chuyển văn bản";
  displayFollowDepartment.value = true;
  files = [];
  listFileUploaded.value = [];
  followdepartment_item.value = {
    created_date: new Date(),
    created_date_str: moment(new Date()).format("DD/MM/YYYY"),
    follow_id: selectedDoc.value.follow_id,
    doc_master_id: selectedDoc.value.doc_master_id,
    organization_id: options.value.organization_id,
    is_deadline: false,
    trackers: [],
    track_departments: {},
    main_departments: {}
  };
}
const saveFollowDepartment = () => {
  submitted.value = true;
  if (Object.keys(followdepartment_item.value.main_departments).length === 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Hãy xác định phòng ban nhận!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (followdepartment_item.value.is_deadline && moment(new Date()).isAfter(followdepartment_item.value.deadline_date, 'day')) {
    swal.fire({
      title: "Thông báo!",
      text: "Hạn xử lý không thể nhỏ hơn ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  let main_dep_ids = [];
  for(var key in followdepartment_item.value.main_departments){
    if(selectedDoc.value.is_drafted){
      if(followdepartment_item.value.main_departments[key]){
        main_dep_ids.push(key);
      }
    }
    else{
      if(followdepartment_item.value.main_departments[key].checked){
        main_dep_ids.push(key);
      }
    }
  }
  let track_dep_ids = [];
  if(Object.keys(followdepartment_item.value.track_departments).length > 0){
    for(var key in followdepartment_item.value.track_departments){
    if(followdepartment_item.value.track_departments[key].checked){
      track_dep_ids.push(key);
    }
  }
  }
  if (typeof followdepartment_item.value.deadline_date == "string") {
    let startDay = followdepartment_item.value.deadline_date.split("/");
    followdepartment_item.value.deadline_date = new Date(
      startDay[2] + "/" + startDay[1] + "/" + startDay[0]
    );
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  formData.append("followdepartment_item", JSON.stringify(followdepartment_item.value));
  formData.append("main_departments", JSON.stringify(main_dep_ids));
  formData.append("track_departments", JSON.stringify(track_dep_ids));
  formData.append("trackers", JSON.stringify(followdepartment_item.value.trackers));
  formData.append("is_exported_xml", followdepartment_item.value.is_exported_xml ? true : false);
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/FollowDepartment_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật văn bản thành công!");
        closeDialog('duyetphongban', 'reload');
      } else {
        swal.fire({
          title: "Thông báo",
          text: "Xảy ra lỗi khi duyệt phòng ban văn bản.",
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
//---------------- Chuyển Đóng dấu -----------------
const displayTransferStamp = ref(false);
const headerTransferStamp = ref();
const transferstamp_item = ref({});
const openModalTransferStamp = () => {
  headerTransferStamp.value = "Chuyển đóng dấu/vào sổ văn bản";
  displayTransferStamp.value = true;
  files = [];
  listFileUploaded.value = [];
  transferstamp_item.value = {
    created_date: new Date(),
    created_date_str: moment(new Date()).format("DD/MM/YYYY"),
    follow_id: selectedDoc.value.follow_id,
    doc_master_id: selectedDoc.value.doc_master_id,
    organization_id: options.value.organization_id,
    is_deadline: false,
    stamper: {}
  };
}
const saveTransferStamp = () => {
  submitted.value = true;
  if (!transferstamp_item.value.user_id) {
    swal.fire({
      title: "Thông báo!",
      text: "Hãy xác định người đóng dấu!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (transferstamp_item.value.is_deadline && moment(new Date()).isAfter(transferstamp_item.value.deadline_date, 'day')) {
    swal.fire({
      title: "Thông báo!",
      text: "Hạn xử lý không thể nhỏ hơn ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (typeof transferstamp_item.value.deadline_date == "string") {
    let startDay = transferstamp_item.value.deadline_date.split("/");
    transferstamp_item.value.deadline_date = new Date(
      startDay[2] + "/" + startDay[1] + "/" + startDay[0]
    );
  }
  transferstamp_item.value.receive_by = all_users.value.find(x => x.user_id === transferstamp_item.value.user_id).user_key;
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  formData.append("stamp_item", JSON.stringify(transferstamp_item.value));
  formData.append("is_exported_xml", transferstamp_item.value.is_exported_xml ? true : false);
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/TransferStamp_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật văn bản thành công!");
        if (isAddDoc.value) {
          closeDialog('chuyendongdau', 'reload');
        }
        else {
          closeDialog('chuyendongdau');
          // loadDataLaw(true);
          // showDetailLaw(detailLaw.value);
        }
      } else {
        console.log(response.data.ms);
        swal.fire({
          title: "Thông báo",
          text: "Xảy ra lỗi khi chuyển đóng dấu văn bản.",
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
//---------------- Đóng dấu -----------------------
const displayStamp = ref();
const headerStamp = ref();
const stamp_item = ref({});
const activeTabStamp = ref(0);
const goToDetailStamp = () => {
  openModalStamp(selectedDoc.value.nav_type, true);
}
const isEditStampDoc = ref(false);
const openModalStamp = (type, is_edit) => {
  activeTabStamp.value = 0;
  isLoaded.value = false;
  let title = type === 1 ? 'đến' : (type === 2 ? 'đi' : 'nội bộ');
  doc_type_title.value = "Văn bản " + title;
  headerStamp.value = "Cập nhật thông tin đóng dấu";
  displayStamp.value = true;
  files = [];
  file_one = [];
  listFileUploaded.value = [];
  selectedReservationNumber.value = "";
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_master_get_byID",
        par: [
          { par: "doc_master_id", va: selectedDoc.value.doc_master_id },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      let objDoc = data[0][0];
      convertDatefromDB(objDoc);
      if (objDoc.organization_id) {
        let keyobj = {};
        keyobj[objDoc.organization_id] = true;
        objDoc.organization_id = keyobj;
      }
      if (objDoc.department_id) {
        let keyobj = {};
        keyobj[objDoc.department_id] = true;
        objDoc.department_id = keyobj;
      }
      if (objDoc.tags != null && objDoc.tags.length > 1) {
        if (!Array.isArray(objDoc.tags)) {
          objDoc.key_tags = objDoc.tags.split(",");
        }
      }
      if (objDoc.field_name != null && objDoc.field_name.length > 1) {
        if (!Array.isArray(objDoc.field_name)) {
          let listFields = objDoc.field_name.split(",");
          objDoc.key_fields = [];
          listFields.forEach((element, i) => {
            let field = category.value.fields.find(x => x.field_name.trim().toUpperCase() === element.trim().toUpperCase());
            objDoc.key_fields.push(field);
          });
        }
      }

      data[1].forEach((element, i) => {
        element.file_type = element.file_type.toLowerCase();
      });
      listFileUploaded.value = data[1];
      listFileDel.value = data[1];
      objDoc.relate_doc = data[2];
      objDoc.follow_id = selectedDoc.value.follow_id;

      if (!is_edit) {
        isEditStampDoc.value = false;
        let init_doc = {
          receive_date: new Date(),
          doc_date: new Date(),
          issue_place: store.getters.user.organization_name,
          is_auto_num: true,
          is_drafted: false,
          is_inworkflow: false,
          is_stamp: true,
        }
        doc.value = Object.assign(objDoc, init_doc);
        doc.value.dispatch_book_id = category.value.dispatch_books.filter(x => x.nav_type === doc.value.nav_type).length > 0 ? category.value.dispatch_books.filter(x => x.nav_type === doc.value.nav_type)[0].dispatch_book_id : null;
        if(doc.value.is_auto_num){
          changeIsAutoNum();
        }
        generateDocCode();
      }
      else {
        isEditStampDoc.value = true;
        objDoc.is_stamp = true;
        doc.value = objDoc;
      }
      isLoaded.value = true;
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
const saveStamp = () => {
    submitted.value = true;
  if(isLoading.value) return false;
  isLoading.value = true;
  if (doc.value.compendium.length >= 500) {
    swal.fire({
      title: "Thông báo!",
      text: "Trích yếu nội dung không được vượt quá 500 kí tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (!doc.value.is_reservation_number && category.value.reservation_numbers.length > 0 
  && category.value.reservation_numbers.find(x=>x.nav_type === doc.value.nav_type && x.reservation_code === doc.value.doc_code)) {
    swal.fire({
      title: "Thông báo!",
      text: "Số đã được giữ! Không thể chọn",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (typeof doc.value.doc_date == "string") {
    let startDay = doc.value.doc_date.split("/");
    doc.value.doc_date = new Date(
      startDay[2] + "/" + startDay[1] + "/" + startDay[0]
    );
  }
  if (typeof doc.value.receive_date == "string") {
    let endDay = doc.value.receive_date.split("/");
    doc.value.receive_date = new Date(
      endDay[2] + "/" + endDay[1] + "/" + endDay[0]
    );
  }
  if (typeof doc.value.deadline_date == "string") {
    let endDay = doc.value.deadline_date.split("/");
    doc.value.deadline_date = new Date(
      endDay[2] + "/" + endDay[1] + "/" + endDay[0]
    );
  }
  if (typeof doc.value.effective_date == "string") {
    let endDay = doc.value.effective_date.split("/");
    doc.value.effective_date = new Date(
      endDay[2] + "/" + endDay[1] + "/" + endDay[0]
    );
  }
  if (typeof doc.value.expiration_date == "string") {
    let endDay = doc.value.expiration_date.split("/");
    doc.value.expiration_date = new Date(
      endDay[2] + "/" + endDay[1] + "/" + endDay[0]
    );
  }
  if (moment(doc.value.doc_date) > moment(new Date())) {
    swal.fire({
      title: "Thông báo!",
      text: "Ngày văn bản không được vượt quá ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (moment(doc.value.receive_date) > moment(new Date())) {
    swal.fire({
      title: "Thông báo!",
      text: "Ngày đến không được vượt quá ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (doc_type_title.value === 0 && moment(doc.value.receive_date < moment(doc.value.doc_date))) {
    swal.fire({
      title: "Thông báo!",
      text: "Ngày đến không được nhỏ hơn ngày văn bản!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (moment(doc.value.expiration_date) < moment(doc.value.effective_date)) {
    swal.fire({
      title: "Thông báo!",
      text: "Ngày đến không được vượt quá ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (doc.value.hold_time && moment(new Date()).isAfter(doc.value.hold_time)) {
    swal.fire({
      title: "Thông báo!",
      text: "Thời gian tổ chức không được trước thời điểm hiện tại",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  // end valid date
  let formData = new FormData();
  for (var i = 0; i < file_one.length; i++) {
    let file = file_one[i];
    formData.append("doc_file", file);
  }
  if (file_one.length > 0 && isEditStampDoc.value === true) {
    deleteDocMainFile();
  }
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  if (doc.value.key_tags != null) {
    doc.value.tags = doc.value.key_tags.toString();
  }
  if (doc.value.key_fields != null && doc.value.key_fields.length > 0) {
    doc.value.field_name = "";
    doc.value.key_fields.forEach((element, i) => {
      if (doc.value.field_name != "") {
        doc.value.field_name += ',';
      }
      doc.value.field_name += element.field_name;
    });
  }
  else {
    doc.value.field_name = null;
  }

  let listDocRelate = [];
  if (doc.value.relate_doc != null && doc.value.relate_doc != "") {
    if (doc.value.relate_doc.length > 0) {
      doc.value.relate_doc.forEach((element, i) => {
        let relateDoc = { doc_master_id: element.doc_master_id, compendium: element.compendium };
        listDocRelate.push(relateDoc);
      });
    }
  }
  if (doc.value.organization_id) {
    doc.value.organization_id = Object.keys(doc.value.organization_id)[0];
  }
  if (doc.value.department_id) {
    doc.value.department_id = Object.keys(doc.value.department_id)[0];
  }
  formData.append("doc", JSON.stringify(doc.value));
  formData.append("doc_relates", JSON.stringify(listDocRelate));
  formData.append("auto_doc_code", JSON.stringify(auto_doc_code.value));
  formData.append("department_id", JSON.stringify(store.getters.user.department_id));
  formData.append("is_reservation_number", doc.value.is_reservation_number ? true : false);
  if (isEditStampDoc.value) {
    listFileDel.value = listFileDel.value.filter(x => listFileUploaded.value.filter(y => y.file_id == x.file_id).length == 0);
    formData.append("fileUploadOld", JSON.stringify(listFileDel.value));
  };
  if (doc.value.file_path === null) {
    formData.append("docUploadOld", del_docfilepath.value);
  };
  if (watermark.value.wm_images.length > 0) {
    formData.append("waterimages", JSON.stringify(watermark.value.wm_images));
  }
  if (watermark.value.wm_texts.length > 0) {
    formData.append("watertxts", JSON.stringify(watermark.value.wm_texts));
  }
  formData.append("is_stamp", doc.value.is_stamp);
  if (ca_files.value.length > 0) {
    formData.append("ca_files", JSON.stringify(ca_files.value));
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/Stamp_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        if(!response.data.notunique_case){
          swal.close();
          toast.success("Đóng dấu văn bản thành công!");
          closeDialog('dongdau', 'reload');
        }
        else if(response.data.notunique_case === 1){
          swal.fire({
                        icon: 'warning',
                        type: 'warning',
                        title: '',
                        text: 'Số vào khối cơ quan đã tồn tại ! Hệ thống tự động tăng lên số hợp lệ gần nhất !'
                    }).then((result) => {
                        loadCategorys();

                        toast.success("Cập nhật văn bản thành công!");
                        closeDialog('sohoa', 'reload');
                    });
        }
      } 
      else {
        let msg = "";
        switch (response.data.notunique_case) {
          case "2":
            msg = "Số vào sổ đang bị trùng với văn bản khác! Vui lòng nhập lại!";
            break;
          case "3":
            msg = "Số ký hiệu trùng! Vui lòng nhập số khác!";
            break;
          default:
            msg = "Xảy ra lỗi khi cập nhật văn bản.";
            break;
        }
        swal.fire({
          title: "Thông báo",
          text: msg,
          icon: "error",
          confirmButtonText: "OK",
        });
        convertNormalToTreeObj(doc.value,['organization_id','department_id','composing_unit_id']);
      }
    })
    .catch((error) => {
      isLoading.value = false;
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
// Watermark
const watermark = ref({wm_images: [], wm_texts: []});
const ca_files = ref([]);
const type_watermark = ref();
var getWatermark = (type, isFormValid) => {
  type_watermark.value = type;
  if(type === 'stamp'){
    if (!isFormValid) {
      if (activeTabStamp.value === 0) {
        swal.fire({
          title: "Thông báo",
          text: "Chưa điền đầy đủ thông tin văn bản!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
      return;
    }
  }
  if(type === 'stamp' && !doc.value.is_approved){
      saveStamp();
  }
  else{
    swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
    emitter.emit("emitData", { type: "getWatermark", data: null });
  }
}
var returnWatermark = (data) => {
  swal.close();
  if (data) {
        watermark.value = data.watermark_obj;
        ca_files.value = data.ca_files;
        if(type_watermark.value === 'stamp')
        saveStamp();
        else if(type_watermark.value === 'sign')
        {
          if(type_approval.value === 'trinhpheduyet'){
            saveApproval();
          }
          else if(type_approval.value === 'duyetphathanh'){
            savePublishingApproval();
          }
        }
        
      }
}
// Change doc code,date
const changeDocCode = () => {
  if(doc.value.is_approved)
  emitter.emit("emitData", { type: "changeDocCode", data: null });
}
const changeDocDate = () => {
  if(doc.value.is_approved)
  emitter.emit("emitData", { type: "changeDocDate", data: null });
}
const changeDocSigner = () => {
  if(doc.value.is_approved)
  emitter.emit("emitData", { type: "changeDocSigner", data: null });
}
//---------------- Xác nhận hoàn thành -------------------
const displayCompleted = ref();
const headerCompleted = ref();
const completed_item = ref({});
const openModalCompleted = () => {
  headerCompleted.value = "Xác nhận hoàn thành công việc của văn bản";
  displayCompleted.value = true;
  files = [];
  listFileUploaded.value = [];
  completed_item.value = {
    created_date: new Date(),
    created_date_str: moment(new Date()).format("DD/MM/YYYY"),
    follow_id: selectedDoc.value.follow_id,
    doc_master_id: selectedDoc.value.doc_master_id,
    organization_id: options.value.organization_id,
  };
}
const saveCompleted = () => {
  submitted.value = true;
  if (!completed_item.value.message && listFileUploaded.value.length === 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Bắt buộc phải có nội dung hoặc file đính kèm !",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  formData.append("completed_item", JSON.stringify(completed_item.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/Completed_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Xác nhận hoàn thành văn bản thành công!");
        closeDialog('xacnhanhoanthanh', 'reload');
      } else {
        console.log(response.data.ms);
        swal.fire({
          title: "Thông báo",
          text: "Xảy ra lỗi khi xác nhận hoàn thành văn bản.",
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
//---------------- Trả lại -------------------
const displayReturn = ref();
const headerReturn = ref();
const return_item = ref({});
const openModalReturn = () => {
  headerReturn.value = "Trả lại văn bản";
  displayReturn.value = true;
  files = [];
  listFileUploaded.value = [];
  return_item.value = {
    created_date: new Date(),
    created_date_str: moment(new Date()).format("DD/MM/YYYY"),
    follow_id: selectedDoc.value.follow_id,
    doc_master_id: selectedDoc.value.doc_master_id,
    organization_id: options.value.organization_id,
  };
}
const saveReturn = () => {
  submitted.value = true;
  if (!return_item.value.message) {
    swal.fire({
      title: "Thông báo!",
      text: "Bắt buộc phải có nội dung trả lại !",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  formData.append("return_item", JSON.stringify(return_item.value));
  formData.append("is_exported_xml", return_item.value.is_exported_xml ? true : false);
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/Return_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Trả lại văn bản thành công!");
        if (isAddDoc.value) {
          closeDialog('tralai', 'reload');
        }
        else {
          closeDialog('tralai');
          // loadDataLaw(true);
          // showDetailLaw(detailLaw.value);
        }
      } else {
        console.log(response.data.ms);
        swal.fire({
          title: "Thông báo",
          text: "Xảy ra lỗi khi trả lại văn bản.",
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
//---------------- Trinh phe duyet ------------------
const displayApproval = ref();
const headerApproval = ref();
const approval_item = ref({});
const iframe_files = ref([]);
const type_approval = ref();
const openModalApproval = () => {
  if(!selectedDoc.value.file_path){
    swal.fire({
          title: "Thông báo",
          text: "Chưa có văn bản đính kèm!",
          icon: "error",
          confirmButtonText: "OK",
        });
    return false;
  }
  type_approval.value = "trinhpheduyet";
  isLoaded.value = false;
  headerApproval.value = "Trình phê duyệt văn bản";
  displayApproval.value = true;
  files = [];
  listFileUploaded.value = [];
  approval_item.value = {
    created_date: new Date(),
    created_date_str: moment(new Date()).format("DD/MM/YYYY"),
    follow_id: selectedDoc.value.follow_id,
    doc_master_id: selectedDoc.value.doc_master_id,
    organization_id: options.value.organization_id,
    is_deadline: false
  };
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_master_get_byID",
        par: [
          { par: "doc_master_id", va: selectedDoc.value.doc_master_id },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      data[1].forEach((element, i) => {
        element.file_type = element.file_type.toLowerCase();
      });
      iframe_files.value = data[1];
      isLoaded.value = true;
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
const openModalPublishingApproval = () => {
  if(!selectedDoc.value.file_path){
    swal.fire({
          title: "Thông báo",
          text: "Chưa có văn bản đính kèm!",
          icon: "error",
          confirmButtonText: "OK",
        });
    return false;
  }
  type_approval.value = "duyetphathanh";
  isLoaded.value = false;
  headerApproval.value = "Duyệt phát hành văn bản";
  displayApproval.value = true;
  files = [];
  listFileUploaded.value = [];
  approval_item.value = {
    created_date: new Date(),
    created_date_str: moment(new Date()).format("DD/MM/YYYY"),
    follow_id: selectedDoc.value.follow_id,
    doc_master_id: selectedDoc.value.doc_master_id,
    organization_id: options.value.organization_id,
    is_deadline: false,
    show_signby: false,
    is_signed: selectedDoc.value.is_signed
  };
  let vanthus = all_users.value.filter(x=>x.role_id === 'vanthu');
  if(vanthus && vanthus.length > 0){
    let ord_vanthus = vanthus.sort((a, b) => parseFloat(a.is_order) - parseFloat(b.is_order));
    approval_item.value.receive_by = ord_vanthus[0].user_key;
  }
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_master_get_byID",
        par: [
          { par: "doc_master_id", va: selectedDoc.value.doc_master_id },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      data[1].forEach((element, i) => {
        element.file_type = element.file_type.toLowerCase();
      });
      iframe_files.value = data[1];
      isLoaded.value = true;
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
const saveApproval = () => {
  submitted.value = true;
  if (!approval_item.value.receive_by) {
    swal.fire({
      title: "Thông báo!",
      text: "Hãy xác định người nhận!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (approval_item.value.is_deadline && moment(new Date()).isAfter(approval_item.value.deadline_date, 'day')) {
    swal.fire({
      title: "Thông báo!",
      text: "Hạn xử lý không thể nhỏ hơn ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  formData.append("approval_item", JSON.stringify(approval_item.value));
  if (watermark.value.wm_images.length > 0) {
    formData.append("waterimages", JSON.stringify(watermark.value.wm_images));
  }
  if (watermark.value.wm_texts.length > 0) {
    formData.append("watertxts", JSON.stringify(watermark.value.wm_texts));
  }
  if (watermark.value.sign_files.length > 0) {
    formData.append("type_signs", JSON.stringify(watermark.value.sign_files));
  }
  if (ca_files.value.length > 0) {
    formData.append("ca_files", JSON.stringify(ca_files.value));
  }
  formData.append("is_exported_xml", approval_item.value.is_exported_xml ? true : false);
  if(isLoading.value) return false;
  isLoading.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/Approval_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật văn bản thành công!");
        closeDialog('pheduyet', 'reload');
        isLoading.value = false;
      } else {
        console.log(response.data.ms);
        isLoading.value = false;
        swal.fire({
          title: "Thông báo",
          text: "Xảy ra lỗi khi cập nhật văn bản.",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      isLoading.value = false;
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
const savePublishingApproval = () => {
  submitted.value = true;
  if (!approval_item.value.receive_by) {
    swal.fire({
      title: "Thông báo!",
      text: "Hãy xác định người nhận!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (approval_item.value.is_deadline && moment(new Date()).isAfter(approval_item.value.deadline_date, 'day')) {
    swal.fire({
      title: "Thông báo!",
      text: "Hạn xử lý không thể nhỏ hơn ngày hiện tại!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  if (!approval_item.value.is_signed && watermark.value.wm_images.find(x=>x[selectedDoc.value.doc_master_id]) && selectedDoc.value.status_id !== 'tralai') {
    swal.fire({
      title: "Thông báo!",
      text: "Chưa chèn chữ ký vào văn bản !",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  formData.append("approval_item", JSON.stringify(approval_item.value));
  if (watermark.value.wm_images.length > 0) {
    formData.append("waterimages", JSON.stringify(watermark.value.wm_images));
  }
  if (watermark.value.wm_texts.length > 0) {
    formData.append("watertxts", JSON.stringify(watermark.value.wm_texts));
  }
  if (ca_files.value.length > 0) {
    formData.append("ca_files", JSON.stringify(ca_files.value));
  }
  formData.append("esim_sign", JSON.stringify(approval_item.value));
  formData.append("is_exported_xml", approval_item.value.is_exported_xml ? true : false);
  if(isLoading.value) return false;
  isLoading.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseUrlCheck +
      `/api/DocMain/PublishingApproval_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật văn bản thành công!");
        closeDialog('pheduyet', 'reload');
        isLoading.value = false;
      } else {
        isLoading.value = false;
        console.log(response.data.ms);
        swal.fire({
          title: "Thông báo",
          text: response.data.ms ? response.data.ms : "Xảy ra lỗi khi cập nhật văn bản.",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      isLoading.value = false;
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
const changeTypeSignature = (is_flashsign) => {
  let type_emit = is_flashsign ? 0 : 1;
  emitter.emit("emitData", { type: "changeTypeSignature", data: type_emit });
}
// --------------- Link, copy vao kho du lieu ---------------------
const displayAddStore = ref(false);
const typeShare = ref();
const DocSelected_ID = ref();
const Doc_Compendium = ref();
const moveToStore = (data, type)=>{
  displayAddStore.value = true;
  typeShare.value = type;
  DocSelected_ID.value = data.doc_master_id;
  Doc_Compendium.value = data.compendium;
}
// -------------- Liên kết công việc ----------------
const headerDialogLinkTask = ref();
const displayDialogLinkTask = ref(false);
const CloseDiaLogLinkTask = () => {
  displayDialogLinkTask.value = false;
}
const showModalLinkTask = () => {
  headerDialogLinkTask.value = "Công việc của tôi";
  displayDialogLinkTask.value = true;
}
//-------------- Trục liên thông ----------------
// Gửi
const openModalSendDocConnect = () => {
  displayDocConnectSend.value = true;
  forceRerender();
}
const closeDialogDocConnectSend = () => {
  displayDocConnectSend.value = false;
  forceRerender();
}
const displayDocConnectSend = ref(false);
//-------------- Reservation Number --------------
const displayReservationNumber = ref(false);
const headerReservationNumber = ref();
const reservation_numbers = ref([]);
const openModalReservationNumber = () => {
  displayReservationNumber.value = true;
  headerReservationNumber.value = "Danh sách số đã giữ";
  reservation_numbers.value = category.value.reservation_numbers.filter(x=>x.nav_type === doc.value.nav_type);
}
const selectedReservationNumber = ref();
const pickReservationNumber = () => {
  let re_num = reservation_numbers.value.find(x=>x.reservation_number_id == selectedReservationNumber.value);
  if(re_num){
    doc.value.doc_code = re_num.reservation_code;
    doc.value.is_reservation_number = true;
  }
  else{
    swal.fire({
          title: "Thông báo!",
          text: "Không có số nào được chọn",
          icon: "error",
          confirmButtonText: "OK",
        });
        return false;
  }
  closeDialog('laysogiu');
}
const cancelReservationNumber = () => {
  doc.value.is_reservation_number = false;
}
// File
let files = [];
let file_one = [];
const listFileUploaded = ref([]); // danh sách file đã up 
const RequiredFileUploaded = ref([]); // danh sách file required đã up 
const listFileDel = ref([]); // danh sách file bị xóa khi cập nhật
const onUploadFile = (event) => {
  files = [];
  event.files.forEach((element) => {
    files.push(element);
  });
};
const onUploadOneFile = (event) => {
  file_one = [];
  event.files.forEach((element) => {
    file_one.push(element);
  });
};
const removeOneFile = (event) => {
  file_one = [];
};
const removeFile = (event) => {
  files = [];
  event.files.forEach((element) => {
    files.push(element);
  });
};
const del_docfilepath = ref();
const deleteDocMainFile = () => {
  del_docfilepath.value = doc.value.file_path;
  doc.value.file_path = null;
  doc.value.file_name = null;
  doc.value.file_path_temp = null;
  doc.value.file_size = null;
  doc.value.file_type = null;
}
const deleteDocRelFile = (value) => {
  listFileUploaded.value = listFileUploaded.value.filter(x => x.file_id != value.file_id);
}
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
// Remove TreeSelect
const removeTreeSelect = (var_name, value) => {
  var_name[value] = null;
  if(value == "department_id" && displayDocSohoa.value){
    generateDocCode();
  }
}
onMounted(() => {
  //init
  loadUserFunctions();
  loadCategorys();
  loadOrgs();
  loadUsers();
  loadDocRoleGroups();
  // Emit
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "goToDetailDoc":
      if (obj.data) {
        let objdoc = obj.data;
        if (objdoc.status_id === 'sohoa' || objdoc.status_id === 'duthao') {
          allowDelDoc.value = true;
          doc_del.value = obj.data;
        }
        else allowDelDoc.value = false;
        loadUserFunctions(obj.data);
        selectedDoc.value = obj.data;
      }
      break;
    case "cancalShareFile":
      if (obj.data) {
        displayAddStore.value = obj.data.displayAddStore;
      }
      break; 
    case 'returnCountCheckbox':
      if(obj.data != null){
        returnCountCheckboxDocs(obj.data.count, obj.data.checked_docs);
      }
    default: break;
  }
  });
  return {
    options,
    isFirst,
    openModalSohoa
  };
});
</script>
<template>
  <div class="surface-100 doc-wrap">
    <div class="doc-header surface-0">
      <Toolbar class="w-full custoolbar">
        <template #start>
          <DocFilter class="mr-2" :Type="'receive'"></DocFilter>
          <DocAdditionalCount />
        </template>

        <template #end>
          <ButtonWrapper v-for="btn in doc_functions">
            <SplitButton :class="{ 'p-button-outlined p-button-secondary': btn.typeBtn === 'outline' }" :label="btn.label"
              :icon="btn.icon" :model="btn.items" class="mr-2" v-if="btn.items && btn.items.length > 0" :key="btn.id">
            </SplitButton>
            <Button :class="{ 'p-button-outlined p-button-secondary': btn.typeBtn === 'outline' }" :label="btn.label"
              :icon="btn.icon" class="mr-2" v-if="!btn.items" @click="btn.command" key="btn.id">
            </Button>
          </ButtonWrapper>
          <Button class="mr-2 p-button-outlined p-button-secondary" icon="pi pi-refresh" @click="onRefresh" />
          <!-- <Button label="Tiện ích" icon="pi pi-file-excel" class="mr-2 p-button-outlined p-button-secondary" -->
          <!-- @click="toggleExport" aria-haspopup="true" aria-controls="overlay_Export" /> -->
          <!-- <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" /> -->
          <Button label="Xoá" icon="pi pi-trash" class="mr-2 p-button-danger" @click="delDoc()" v-if="allowDelDoc" />
        </template>
      </Toolbar>
    </div>
    <div class="doc-body surface-0">
      <Splitter class="w-full">
        <SplitterPanel :size="40">
          <DocList :Type="'receive'" />
        </SplitterPanel>
        <SplitterPanel :size="60">
          <DocDetail :Type="'receive'" :goToViewDoc="openModalViewDoc" :goToDetailSohoa="goToDetailSohoa" :goToDetailDuthao="goToDetailDuthao" :goToDetailStamp="goToDetailStamp"/>
        </SplitterPanel>
      </Splitter>
    </div>
  </div>
  <Dialog class="black-text" :header="headerDoc" v-model:visible="displayDocSohoa" :maximizable="true" :autoZIndex="true"
    :style="{ width: '70vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-8 md:col-8 m-0 p-0">
            <div v-if="doc.nav_type !== 1" class="col-12 md:col-12 m-0 p-0 flex">
              <label class="col-3 p-0 text-left flex" style="align-items:center;">
                Văn bản của phòng ban
              </label>
              <InputSwitch @change="changeIsByDepartment" v-model="doc.is_by_department" class="col-9" />
            </div>
            <div v-if="!doc.is_by_department" class="field col-12 md:col-12 p-0 flex" :class="{'mt-3': doc.nav_type !== 1}">
              <label class="col-3 p-0 text-left flex" style="align-items:center;">
                Đơn vị
                <span class="redsao pl-1"> (*)</span>
              </label>
              <TreeSelect @change="generateDocCode" class="col-8 p-0" v-model="doc.organization_id" :options="treeorgs" :showClear="true"
                :max-height="200" placeholder="Chọn đơn vị">
              </TreeSelect>
              <!-- <Dropdown class="col-8 p-0" spellcheck="false" v-model="doc.organization_id" :options="treeorgs"
              optionLabel="organization_name" optionValue="organization_id" :editable="false" :filter="true" /> -->
            </div>
            <div v-if="doc.is_by_department" class="field col-12 md:col-12 m-0 p-0 flex mt-3">
              <label class="col-3 p-0 text-left flex" style="align-items:center;">
                Phòng ban <span class="redsao pl-1"> (*)</span>
              </label>
              <TreeSelectCustom @change="generateDocCode" class="col-8 p-0" :removeTree="removeTreeSelect" :model="doc" property-name="department_id" :options="category.departments"
                placeholder="Chọn phòng ban">
              </TreeSelectCustom>
            </div>
            <div class="col-12 md:col-12 m-0 p-0 flex mt-3">
              <div class="col-5 md:col-5 m-0 p-0 flex">
                  <label class="col-7 p-0 text-left flex" style="align-items:center;">
                  Vào số tự động
                  </label>
                  <InputSwitch @change="changeIsAutoNum" v-model="doc.is_auto_num" class="col-4 mt-1" />
              </div>
              <div v-if="doc.nav_type === 1" class="col-6 md:col-6 m-0 p-0 flex">
                  <label class="col-4 text-left flex p-0" style="align-items:center;">
                    Ngày đến
                  </label>
                  <Calendar class="col-8 p-0" id="date_publish" v-model="doc.receive_date" :manualInput="true"
                    :showIcon="true" />
              </div>
              <div v-if="doc.nav_type !== 1" class="col-6 md:col-6 m-0 p-0 flex">
              <label class="col-4 text-left flex" style="align-items:center;">
                Ngày văn bản
                <!-- <span class="redsao pl-1"> (*)</span> -->
              </label>
              <Calendar @input="autoFillDate(doc, 'doc_date')" id="doc_date" :showOnFocus="false" class="col-8 p-0" v-model="doc.doc_date" :manualInput="true" :showIcon="true" />
            </div>
            </div>
          </div>
          <div class="col-4 md:col-4 p-0 m-0 flex">
            <button type="button" class="btn"
              style="border: none; padding-left:10px;height:100%;width:100%;background-color:#ccebff;cursor: default">{{ doc_type_title }}</button>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Khối cơ quan
            </label>
            <Dropdown class="col-7 p-0" spellcheck="false" v-model="doc.dispatch_book_id"
            @change="generateDocCode(); changeIsAutoNum()"
              :options="category.dispatch_books.filter(x => x.nav_type === doc.nav_type)"
              optionLabel="dispatch_book_name" optionValue="dispatch_book_id" :editable="false" :filter="true" />
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 text-left flex" style="align-items:center;">
              {{doc.nav_type !== 1 ? 'Số vào sổ' : 'Số đến'}}
              <span class="redsao pl-1"> (*)</span>
            </label>
            <InputText @change="generateDocCode" :disabled="doc.is_auto_num" v-model="doc.dispatch_book_code" class="col-8 p-0 ip36 dispatch-input"
              :class="{ 'p-invalid': v$.dispatch_book_code.$invalid && submitted }" :useGrouping="false" />
          </div>
        </div>
      <receive class="col-12 md:col-12 p-0" v-if="doc.nav_type === 1">
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 p-0 m-0 flex"></div>
          <div class="col-6 md:col-6 p-0 m-0 flex"
            v-if="(v$.dispatch_book_code.$invalid && submitted) || v$.dispatch_book_code.$pending.$response">
            <div class="col-4 text-left flex"></div>
            <small class="col-8 p-0">
              <span style="color:red" class="col-12 ">{{
                  v$.dispatch_book_code.required.$message
                    .replace("Value", "Số vào sổ")
                    .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Số/Ký hiệu
              <!-- <span class="redsao pl-1"> (*)</span> -->
            </label>
            <div class="p-inputgroup col-7 ip36 p-0">
                    <InputText autofocus @change="cancelReservationNumber" v-model="doc.doc_code"
                     />
                    <Button tabindex="-1" @click="openModalReservationNumber" v-tooltip.right="'Lấy từ danh sách giữ số'" icon="pi pi-list"/>
            </div>
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 text-left flex" style="align-items:center;">
              Ngày văn bản
              <!-- <span class="redsao pl-1"> (*)</span> -->
            </label>
            <Calendar @input="autoFillDate(doc,'doc_date')" id="doc_date" :showOnFocus="false" class="col-8 p-0" v-model="doc.doc_date" :manualInput="true" :showIcon="true" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Trích yếu
              <span class="redsao pl-1"> (*)</span>
            </label>
            <Textarea v-model="doc.compendium" spellcheck="false" class="col-10 ip36" autoResize
              :class="{ 'p-invalid': v$.compendium.$invalid && submitted }" style="padding:0.5rem;" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex"
            v-if="(v$.compendium.$invalid && submitted) || v$.compendium.$pending.$response">
            <div class="col-2 text-left flex"></div>
            <small class="col-10 p-0">
              <span style="color:red" class="col-12 ">{{
                  v$.compendium.required.$message
                    .replace("Value", "Trích yếu")
                    .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>
        <!-- <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 p-0 m-0 flex">
            <div class="col-12 md:col-12 p-12 m-12 flex" 
            v-if="(v$.doc_code.$invalid && submitted) || v$.doc_code.$pending.$response">
            <div class="col-4 text-left flex"></div>
            <small class="col-8 p-0">
              <span style="color:red" class="col-12 ">{{
                  v$.doc_code.required.$message
                    .replace("Value", "Số ký hiệu văn bản")
                    .replace("is required", "không được để trống")
              }}</span>
            </small>
            </div>
          </div>
          <div class="col-6 md:col-6 p-0 m-0 flex"
            v-if="((v$.doc_date.$invalid && submitted) || v$.doc_date.$pending.$response)">
            <div class="col-4 p-0 text-left flex"></div>
            <small class="col-8 p-0">
              <span style="color:red" class="col-12 ">{{
                  v$.doc_date.required.$message
                    .replace("Value", "Ngày văn bản")
                    .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div> -->
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <div class="field col-12 md:col-12 m-0 p-0 flex">
              <div class="col-7 md:col-7 m-0 p-0 flex">
                <label class="col-7 m-0 p-0 text-left flex" style="align-items:center;margin-right: -3px !important;">
                  Số lượng bản
                </label>
                <InputNumber v-model="doc.num_of_copies" class="col-4 p-0 ip36" :useGrouping="false" />
              </div>
              <div class="col-5 md:col-5 m-0 p-0 flex">
                <label class="col-4 m-0 p-0 text-left flex" style="align-items:center;">
                  Số trang
                </label>
                <InputNumber v-model="doc.num_of_pages" style="padding-right: 5px !important;" class="col-6 p-0 ip36" :useGrouping="false" />
              </div>
            </div>
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <div class="field col-12 md:col-12 m-0 p-0 flex">
              <div class="col-7 md:col-7 m-0 flex">
                <label class="col-7 m-0 p-0 text-left flex" style="align-items:center;">
                  Độ mật
                </label>
                <Dropdown :showClear="true" class="col-5 p-0" spellcheck="false" v-model="doc.security" :options="category.security"
              optionLabel="security_name" optionValue="security_name" :editable="false" :filter="true" />
              </div>
              <div class="col-5 md:col-5 m-0 p-0 pl-2 flex">
                <label class="col-5 m-0 p-0 text-left flex" style="align-items:center;">
                  Độ khẩn
                </label>
                <Dropdown :showClear="true" class="col-7 p-0" spellcheck="false" v-model="doc.urgency" :options="category.urgency"
              optionLabel="urgency_name" optionValue="urgency_name" :editable="false" :filter="true" />
              </div>
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Nhóm văn bản
            </label>
            <Dropdown :showClear="true" @change="changeDocGroup(); generateDocCode()" class="col-7 p-0" spellcheck="false" v-model="doc.doc_group_id"
              :options="category.groups" optionLabel="doc_group_name" optionValue="doc_group_id" :editable="false"
              :filter="true" />
          
          </div>
          <div class="col-6 md:col-6 p-0 m-0 flex">
            <label class="col-4 text-left flex" style="align-items:center;">
                  Không gửi bản giấy
                </label>
                <InputSwitch v-model="doc.is_not_send_paper" class="col-8" />
          </div>
        </div>
        <!-- <div v-if="doc.nav_type !== 1" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Nhóm văn bản
            </label>
            <Dropdown :showClear="true" @change="changeDocGroup(); generateDocCode()" class="col-10 p-0" spellcheck="false" v-model="doc.doc_group_id"
              :options="category.groups" optionLabel="doc_group_name" optionValue="doc_group_id" :editable="false"
              :filter="true" />
          </div>
        </div> -->
        <div v-if="doc.nav_type === 1" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Nơi ban hành
              <span class="redsao pl-1"> (*)</span>
            </label>
            <Dropdown panelClass="d-design-dropdown" @blur="autoFillIssuePlace(doc.issue_place, doc, 'issue_place')" class="col-10 p-0" spellcheck="false" v-model="doc.issue_place" :options="category.issue_places"
              optionLabel="issue_place_name" optionValue="issue_place_name" :editable="true" :filter="true">
                <template #option="slotProps">
                  <div :style="{'padding-left': slotProps.option.level > 1 ? ((slotProps.option.level-1) + 'rem') : '0'}">{{slotProps.option.issue_place_name}}</div>
              </template>
            </Dropdown>
            <!-- <Dropdown class="col-10 p-0" spellcheck="false" v-model="doc.issue_place" :options="category.issue_places"
              optionLabel="issue_place_name" optionValue="issue_place_name" :editable="true" :filter="true" /> -->
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex"
            v-if="(v$.issue_place.$invalid && submitted) || v$.issue_place.$pending.$response">
            <div class="col-2 text-left flex"></div>
            <small class="col-10 p-0">
              <span style="color:red" class="col-12 ">{{
                  v$.issue_place.required.$message
                    .replace("Value", "Nơi ban hành")
                    .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Người ký
            </label>
                  <Dropdown v-if="doc.nav_type === 1" @change="changeSigner" :filter="true" v-model="doc.signer" :editable="true"
              :options="category.signers" optionValue="signer_name" optionLabel="signer_name" class="col-7 mt-2 p-0"
              >
            </Dropdown>
              <Dropdown v-if="doc.nav_type !== 1" @change="changeSigner" :filter="true" v-model="doc.signer" :editable="true"
              :options="all_users" optionValue="full_name" optionLabel="full_name" class="col-7 mt-2 p-0"
              >
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </Dropdown>
            <!-- <InputText v-model="doc.signer" class="col-7 ip36" /> -->
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 text-left flex p-0" style="align-items:center;">
              Ngày hạn xử lý
            </label>
            <Calendar :showOnFocus="false" @input="autoFillDate(doc,'deadline_date')" id="deadline_date" class="col-8 p-0" v-model="doc.deadline_date" :manualInput="true"
              :showIcon="true" />
          </div>
        </div>
      </receive>
      <send class="col-12 md:col-12 p-0" v-if="doc.nav_type !== 1">
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Nhóm văn bản
            </label>
            <Dropdown :showClear="true" @change="changeDocGroup(); generateDocCode()" class="col-7 p-0" spellcheck="false" v-model="doc.doc_group_id"
              :options="category.groups" optionLabel="doc_group_name" optionValue="doc_group_id" :editable="false"
              :filter="true" />
          
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Nơi soạn thảo
            </label>
            <TreeSelectCustom class="col-8 p-0" :removeTree="removeTreeSelect" :model="doc" property-name="composing_unit_id" :options="category.departments"
                placeholder="Chọn nơi soạn thảo">
              </TreeSelectCustom>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Người soạn thảo
            </label>
            <InputText v-model="doc.drafter" class="col-7 ip36" />
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Số/Ký hiệu
              <!-- <span class="redsao pl-1"> (*)</span> -->
            </label>
            <div class="p-inputgroup col-8 ip36 p-0">
                    <InputText autofocus @change="cancelReservationNumber" v-model="doc.doc_code"
                     />
                    <Button tabindex="-1" @click="openModalReservationNumber" v-tooltip.right="'Lấy từ danh sách giữ số'" icon="pi pi-list"/>
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Trích yếu
              <span class="redsao pl-1"> (*)</span>
            </label>
            <Textarea v-model="doc.compendium" spellcheck="false" class="col-10 ip36" autoResize
              :class="{ 'p-invalid': v$.compendium.$invalid && submitted }" style="padding:0.5rem;" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 m-0 p-0 text-left flex" style="align-items:center;">
                  Độ khẩn
                </label>
                <Dropdown :showClear="true" class="col-7 p-0" spellcheck="false" v-model="doc.urgency" :options="category.urgency"
              optionLabel="urgency_name" optionValue="urgency_name" :editable="false" :filter="true" />
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Người ký
            </label>
                  <Dropdown v-if="doc.nav_type === 1" @change="changeSigner" :filter="true" v-model="doc.signer" :editable="true"
              :options="category.signers" optionValue="signer_name" optionLabel="signer_name" class="col-8 mt-2 p-0"
              >
            </Dropdown>
              <Dropdown v-if="doc.nav_type !== 1" @change="changeSigner" :filter="true" v-model="doc.signer" :editable="true"
              :options="all_users" optionValue="full_name" optionLabel="full_name" class="col-8 mt-2 p-0"
              >
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-8 m-0 p-0 text-left flex" style="align-items:center;margin-right: -3px !important;">
                  Số lượng bản
                </label>
                <InputNumber v-model="doc.num_of_copies" class="col-4 pl-1 p-0 ip36" :useGrouping="false" />
          </div>
          <div class="col-5 md:col-5 m-0 p-0 flex">
            <label class="col-7 pl-5 p-0 text-left flex" style="align-items:center;">
              Số trang
            </label>
            <InputNumber v-model="doc.num_of_pages" class="col-5 p-0 ip36" :useGrouping="false" />
          </div>
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 m-0 p-0 text-left flex" style="align-items:center;">
                  Độ mật
                </label>
                <Dropdown :showClear="true" class="col-8 p-0" spellcheck="false" v-model="doc.security" :options="category.security"
              optionLabel="security_name" optionValue="security_name" :editable="false" :filter="true" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Nơi nhận qua mạng
            </label>
            <div class="col-10 p-0">
              <Dropdown :disabled="doc.receive_place_online_checkbox && doc.receive_place_online_checkbox.length > 0" placeholder="Nhấn Enter để thêm" panelClass="d-design-dropdown" @keydown="autoFillReceivePlaceOnline($event,doc, 'receive_place_online')" class="col-12 p-0" v-model="doc.filter_receive_place_online" spellcheck="false" :options="category.issue_places"
              optionLabel="issue_place_name" optionValue="issue_place_name" :editable="true" :filter="true">
                <template #option="slotProps">
                  <div :style="{'padding-left': slotProps.option.level > 1 ? ((slotProps.option.level-1) + 'rem') : '0'}">{{slotProps.option.issue_place_name}}</div>
              </template>
            </Dropdown>
            <MultiSelect :disabled="doc.receive_place_online_checkbox && doc.receive_place_online_checkbox.length > 0" :showToggleAll="false" class="col-12 wrap-multi ip36" :options="doc.arr_receive_place_online" optionLabel="value" v-model="doc.arr_receive_place_online" display="chip"/>
            <!-- <InputText v-model="doc.receive_place_online" class="col-12 ip36"/> -->
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex mt-3" v-if="category.issue_places_checkbox.length > 0">
            <label class="col-2 p-0 text-left flex" style="align-items:center;"></label>
            <div class="col-10 p-0 flex">
              <div v-for="item in category.issue_places_checkbox" :key="item.issue_place_id" class="flex mr-3" style="column-gap: 1rem">
                <label class="text-left mt-1" style="align-items:center;">
                      {{ item.issue_place_name }}
                </label>
                <Checkbox @change="removeReceivePlaceOnline" name="rpo" :value="item.issue_place_name" v-model="doc.receive_place_online_checkbox" />
              </div>
            </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Nơi nhận
            </label>
            <Dropdown @input="changeReceivePlace" @change="fillReceivePlace" class="col-10 p-0" spellcheck="false" v-model="doc.receive_place" :options="category.receive_places"
              optionLabel="receive_place_name" optionValue="receive_place_name" :editable="true" :filter="true" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
            <label class="col-2 text-left flex p-0" style="align-items:center;">
              Ngày hạn xử lý
            </label>
            <Calendar :showOnFocus="false" @input="autoFillDate(doc,'deadline_date')" id="deadline_date" class="col-10 p-0" v-model="doc.deadline_date" :manualInput="true"
              :showIcon="true" />
        </div>
      </send>
        <div class="field col-12 md:col-12 flex" style="justify-content: end">
          <Button class="btn-save-continued" v-if="!isViewDoc" label="Lưu và tiếp tục" icon="pi pi-check" @click="saveDocContinued(!v$.$invalid)" />
        </div>
        <div v-if="doc.nav_type === 1" class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Lĩnh vực
            </label>
            <MultiSelect v-model="doc.key_fields" :options="category.fields" optionLabel="field_name"
              placeholder="Lĩnh vực" :filter="true" display="chip" class="col-7 p-0 text-left" />
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Sao ĐV phối hợp
            </label>
            <InputText v-model="doc.saodv" class="col-8 ip36" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Lãnh đạo
            </label>
            <InputText v-model="doc.ldt" class="col-7 ip36" />
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Chức vụ người ký
            </label>
            <Dropdown class="col-8 p-0" spellcheck="false" v-model="doc.position" :options="category.positions"
              optionLabel="position_name" optionValue="position_name" :editable="true" :filter="true" />
          </div>
        </div>
        <div v-if="doc.nav_type !== 1" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Đơn vị liên quan
            </label>
            <InputText v-model="doc.related_unit" class="col-10 ip36" />
          </div>
        </div>
        <div v-if="doc.nav_type !== 1" class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
                <label class="col-4 m-0 p-0 text-left flex" style="align-items:center;">
                  Số bản lưu
                </label>
                <InputNumber v-model="doc.num_of_saved" class="col-7 p-0 ip36" :useGrouping="false" />
              </div>
              <div class="col-6 md:col-6 m-0 p-0 pl-2 flex">
                <label class="col-4 m-0 p-0 text-left flex" style="align-items:center;">
                  Số bản PH
                </label>
                <InputNumber v-model="doc.num_of_issue" class="col-8 p-0 ip36" :useGrouping="false" />
              </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Thời gian tổ chức
            </label>
            <Calendar :showOnFocus="false" :manualInput="true" :showIcon="true" class="col-7 p-0" v-model="doc.hold_time" :showTime="true" />
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Địa điểm tổ chức
            </label>
            <InputText v-model="doc.hold_place" class="col-8" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Hình thức gửi
            </label>
            <Dropdown class="col-10 p-0" spellcheck="false" v-model="doc.send_way" :options="category.send_ways"
              optionLabel="send_way_name" optionValue="send_way_name" :editable="false" :filter="true" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-2 p-0 mt-3 text-left">Văn bản đính kèm (file số hoá)</label>
            <div class="col-10 p-0">
              <FileUpload chooseLabel="Chọn File" accept=".pdf,.png, .jpg, .jpeg" :showUploadButton="false"
                :showCancelButton="false" :multiple="false" :maxFileSize="10000000000" :fileLimit="1"
                @select="onUploadOneFile" ref="fileupload" @remove="removeOneFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="doc.file_path">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <div class="w-full">
                <Toolbar class="w-full">
                  <template #start>
                    <div class="flex align-items-center">
                      <img class="mr-2" :src="basedomainURL + '/Portals/Image/file/' + doc.file_type + '.png'"
                        style="object-fit: contain;" width="40" height="40" />
                      <span style="line-height:1.5"> {{ doc.file_name }}</span>
                    </div>
                  </template>
                  <template #end>
                    <Button icon="pi pi-times" class="p-button-rounded p-button-danger" @click="deleteDocMainFile()" />
                  </template>
                </Toolbar>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-2 p-0 mt-3 text-left">Tài liệu đính kèm khác</label>
            <div class="col-10 p-0">
              <FileUpload chooseLabel="Chọn File" :showUploadButton="false" :showCancelButton="false" :multiple="true"
                :maxFileSize="10000000000" @select="onUploadFile" @remove="removeFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll" :scrollable="true">
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2"
                            :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                        </div>
                      </template>
                      <template #end>
                        <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                          @click="deleteDocRelFile(slotProps.data)" />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <Accordion :multiple="true">
            <AccordionTab header="THÔNG TIN KHÁC">
              <div class="field col-12 md:col-12 p-0 flex">
                <label class="col-2 mb-0 text-left flex" style="align-items:center;">Văn bản liên quan </label>
                <MultiSelect v-model="doc.relate_doc" :options="allRelateDoc" optionLabel="compendium"
                  placeholder="Văn bản liên quan" display="chip" :filter="true" class="multiselect-custom col-10 p-0">
                  <template #value="slotProps">
                    <div class="p-multiselect-car-token" v-for="option of slotProps.value" :key="option.doc_master_id">
                      <div>{{ option.compendium + (option.doc_code ? (' (' + option.doc_code + ')') : '') }}</div>
                    </div>
                    <template v-if="!slotProps.value || slotProps.value.length === 0">
                      Văn bản liên quan
                    </template>
                  </template>
                  <template #option="slotProps">
                    <div class="country-item" style="overflow:hidden;white-space:normal;">
                      <div>{{ slotProps.option.compendium + (slotProps.option.doc_code ? (' (' + slotProps.option.doc_code
                          + ')') : '')
                      }}</div>
                    </div>
                  </template>
                  <template #footer>
                    <div>
                      <Toolbar>
                        <template #end>
                          <div>
                            <Button icon="pi pi-trash" class="mr-2 p-button-danger" label="Xóa" @click="resetRelate">
                            </Button>
                          </div>
                        </template>
                      </Toolbar>
                    </div>
                  </template>
                </MultiSelect>
              </div>
              <div class="col-12 field md:col-12 flex">
                <div class="col-2 text-left pt-2 p-0" style="text-align: left">
                  Từ khoá
                </div>
                <Chips class="col-10 p-0 text-left" placeholder="Ấn Enter sau mỗi từ khóa" v-model="doc.key_tags" />
              </div>
              <!-- <div class="field col-12 md:col-12 flex">
                <div class="col-6 md:col-6 m-0 p-0 flex">
                  <label class="col-4 text-left flex" style="align-items:center;">
                    Ngày có hiệu lực
                  </label>
                  <Calendar class="col-7 p-0" id="date_publish" v-model="doc.effective_date" :manualInput="true"
                    :showIcon="true" />
                </div>
                <div class="col-6 md:col-6 m-0 p-0 flex">
                  <label class="col-4 text-left flex" style="align-items:center;">
                    Ngày hết hiệu lực
                  </label>
                  <Calendar class="col-8 p-0" id="date_publish" v-model="doc.expiration_date" :manualInput="true"
                    :showIcon="true" />
                </div>
              </div> -->
            </AccordionTab>
            <AccordionTab header="THÔNG TIN LƯU TRỮ">
              <div class="field col-12 md:col-12 flex">
                <div class="col-6 md:col-6 m-0 p-0 flex">
                  <label class="col-4 p-0 text-left flex" style="align-items:center;">
                    Phòng/kho
                  </label>
                  <InputText v-model="doc.warehouse" class="col-7 ip36" />
                </div>
                <div class="col-6 md:col-6 m-0 p-0 flex">
                  <label class="col-4 p-0 text-left flex" style="align-items:center;">
                    Tủ
                  </label>
                  <InputText v-model="doc.cabinet" class="col-8 ip36" />
                </div>
              </div>
              <div class="field col-12 md:col-12 flex">
                <div class="col-6 md:col-6 m-0 p-0 flex">
                  <label class="col-4 p-0 text-left flex" style="align-items:center;">
                    Kệ
                  </label>
                  <InputText v-model="doc.shelf" class="col-7 ip36" />
                </div>
                <div class="col-6 md:col-6 m-0 p-0 flex">
                  <label class="col-4 p-0 text-left flex" style="align-items:center;">
                    Hộp
                  </label>
                  <InputText v-model="doc.box" class="col-8 ip36" />
                </div>
              </div>
              <div class="field col-12 md:col-12 flex">
                <div class="col-12 md:col-12 p-0 m-0 flex">
                  <label class="col-2 p-0 text-left flex" style="align-items:center;">
                    Ghi chú
                  </label>
                  <Textarea v-model="doc.notes" spellcheck="false" class="col-10 ip36" autoResize autofocus
                    style="padding:0.5rem;" />
                </div>
              </div>
            </AccordionTab>
          </Accordion>
        </div>

      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('sohoa')" class="p-button-text" />
      <Button v-if="!isViewDoc" label="Lưu" icon="pi pi-check" @click="saveDoc(!v$.$invalid)" />
    </template>
  </Dialog>
  <Dialog class="black-text" :header="headerDoc" v-model:visible="displayDocDuthao" :maximizable="true" :autoZIndex="true"
    :style="{ width: '60vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-8 md:col-8 m-0 p-0">
            <div class="col-12 md:col-12 m-0 p-0 flex">
              <label class="col-3 p-0 text-left flex" style="align-items:center;">
                Văn bản của phòng ban
              </label>
              <InputSwitch v-model="doc.is_by_department" class="col-9" />
              </div>
            <div v-if="!doc.is_by_department" class="col-12 md:col-12 p-0 flex mt-3">
              <label class="col-3 p-0 text-left flex" style="align-items:center;">
                Đơn vị thực hiện
                <span class="redsao pl-1"> (*)</span>
              </label>
              <TreeSelect class="col-8 p-0" v-model="doc.organization_id" :options="treeorgs" :showClear="true"
                :max-height="200" placeholder="Chọn đơn vị">
              </TreeSelect>
            </div>
            <div class="col-12 md:col-12 m-0 p-0 flex mt-3">
              <label class="col-3 p-0 text-left flex" style="align-items:center;">
                Phòng ban soạn lưu
              </label>
              <TreeSelectCustom :disabled="!isVT" class="col-8 p-0" :removeTree="removeTreeSelect" :model="doc" property-name="department_id" :options="category.departments"
                placeholder="Chọn phòng ban">
              </TreeSelectCustom>
              <!-- <Dropdown placeholder="Chọn phòng ban" class="col-8 p-0" spellcheck="false" v-model="doc.department_id" :options="category.departments"
              optionLabel="organization_name" optionValue="organization_id" :editable="false" :filter="true" /> -->
            </div>
          </div>
          <div class="col-4 md:col-4 p-0 m-0 flex">
            <button type="button" class="btn"
              style="border: none; padding-left:10px;height:100%;width:100%;background-color:#ccebff;cursor: default">{{ doc_type_title }}</button>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Người soạn thảo
            </label>
            <InputText v-model="doc.drafter" class="col-10 ip36" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Trích yếu
              <span class="redsao pl-1"> (*)</span>
            </label>
            <Textarea v-model="doc.compendium" spellcheck="false" class="col-10 ip36" autoResize autofocus
              :class="{ 'p-invalid': v$_dt.compendium.$invalid && submitted }" style="padding:0.5rem;" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex"
            v-if="(v$_dt.compendium.$invalid && submitted) || v$_dt.compendium.$pending.$response">
            <div class="col-2 text-left flex"></div>
            <small class="col-10 p-0">
              <span style="color:red" class="col-12 ">{{
                  v$_dt.compendium.required.$message
                    .replace("Value", "Trích yếu")
                    .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Nhóm văn bản
            </label>
            <Dropdown @change="changeDocGroup" class="col-7 p-0" :showClear="true" spellcheck="false" v-model="doc.doc_group_id"
              :options="category.groups" optionLabel="doc_group_name" optionValue="doc_group_id" :editable="false"
              :filter="true" />
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Lĩnh vực
            </label>
            <MultiSelect v-model="doc.key_fields" :options="category.fields" optionLabel="field_name"
              placeholder="Chọn lĩnh vực" :filter="true" display="chip" class="col-8 p-0 text-left" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-4 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Số trang
            </label>
            <InputNumber v-model="doc.num_of_pages" class="col-4 p-0 ip36" :useGrouping="false" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-2 p-0 mt-3 text-left">Văn bản đính kèm<span class="redsao pl-1">
                (*)</span></label>
            <div class="col-10 p-0">
              <FileUpload chooseLabel="Chọn File" accept=".doc,.docx,.xls,.xlsx" :showUploadButton="false"
                :showCancelButton="false" :multiple="false" :maxFileSize="10000000000" :fileLimit="1"
                @select="onUploadOneFile" @remove="removeOneFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="doc.file_path">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <div class="w-full">
                <Toolbar class="w-full">
                  <template #start>
                    <div class="flex align-items-center">
                      <img class="mr-2" :src="basedomainURL + '/Portals/Image/file/' + doc.file_type + '.png'"
                        style="object-fit: contain;" width="40" height="40" />
                      <span style="line-height:1.5"> {{ doc.file_name }}</span>
                    </div>
                  </template>
                  <template #end>
                    <Button icon="pi pi-times" class="p-button-rounded p-button-danger" @click="deleteDocMainFile()" />
                  </template>
                </Toolbar>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-2 p-0 mt-3 text-left">Tài liệu đính kèm khác</label>
            <div class="col-10 p-0">
              <FileUpload chooseLabel="Chọn File" :showUploadButton="false" :showCancelButton="false" :multiple="true"
                :maxFileSize="10000000000" @select="onUploadFile" @remove="removeFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll" :scrollable="true">
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2"
                            :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                        </div>
                      </template>
                      <template #end>
                        <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                          @click="deleteDocRelFile(slotProps.data)" />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12 p-0 flex">
          <label class="col-2 mb-0 text-left flex" style="align-items:center;">Văn bản liên quan </label>
          <MultiSelect v-model="doc.relate_doc" :options="allRelateDoc" optionLabel="compendium"
            placeholder="Văn bản liên quan" display="chip" :filter="true" class="multiselect-custom col-10 p-0">
            <template #value="slotProps">
              <div class="p-multiselect-car-token" v-for="option of slotProps.value" :key="option.doc_master_id">
                <div>{{ option.compendium }}</div>
              </div>
              <template v-if="!slotProps.value || slotProps.value.length === 0">
                Văn bản liên quan
              </template>
            </template>
            <template #option="slotProps">
              <div class="country-item" style="overflow:hidden;white-space:normal;">
                <div>{{ slotProps.option.compendium }}</div>
              </div>
            </template>
            <template #footer>
              <div>
                <Toolbar>
                  <template #end>
                    <div>
                      <Button icon="pi pi-trash" class="mr-2 p-button-danger" label="Xóa" @click="resetRelate">
                      </Button>
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
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('duthao')" class="p-button-text" />
      <Button v-if="!isViewDoc" label="Lưu" icon="pi pi-check" @click="saveDocDuthao(!v$_dt.$invalid)" />
    </template>
  </Dialog>
  <Dialog :modal="true" :header="headerPublish" v-model:visible="displayPublish" :autoZIndex="true"
    :style="{ width: '50vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Đơn vị nhận &nbsp; <small style="font-weight: 500">(gửi toàn bộ công ty)</small></b>
            </label>
            <TreeSelect class="col-12 ip36 mt-2 p-0 text-left" v-model="publish_item.organizations" :options="treeorgs"
              :showClear="true" :max-height="200" display="chip" selectionMode="checkbox" placeholder="Đơn vị">
            </TreeSelect>
            <!-- <MultiSelect v-model="publish_item.organizations" 
												:options="treeorgs" 
												optionLabel="organization_name" 
												optionValue="organization_id" 
												placeholder="Đơn vị" 
												:filter="true"
												display="chip" 
												class="col-12 mt-2 p-0 text-left"
											/> -->
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Nhóm nhận &nbsp; <small style="font-weight: 500">(gửi đến nhóm đã tạo)</small></b>
            </label>
            <MultiSelect v-model="publish_item.groups" :options="publish_groups" optionLabel="role_group_name"
              optionValue="role_group_id" placeholder="Nhóm" :filter="true" display="chip"
              class="col-12 mt-2 p-0 text-left">
              <template #option="slotProps">
                <div class="country-item flex" style="justify-content: space-between; align-items: center; width: 100%">
                  <div>{{ slotProps.option.role_group_name }}</div>
                  <AvatarGroup class="mt-2" v-if="slotProps.option.users">
                    <Avatar v-tooltip.top="item.full_name" v-for="item in slotProps.option.users.slice(0, 3)"
                      :key="item.role_group_user_id" v-tooltip.left="item.full_name"
                      v-bind:label="item.avatar ? '' : item.full_name.split(' ').at(-1).substring(0, 1)"
                      v-bind:image="basedomainURL + item.avatar"
                      style="background-color: #2196f3; color: #ffffff; vertical-align: middle" class="mr-2"
                      size="small" shape="circle" />
                    <Avatar v-if="slotProps.option.users && slotProps.option.users.length > 3"
                      v-bind:label="'+' + (slotProps.option.users.length - 3).toString()" shape="circle" size="small"
                      style="background-color: #2196f3; color: #ffffff" />
                  </AvatarGroup>
                </div>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Người nhận khác &nbsp; <small style="font-weight: 500">(gửi đích danh)</small></b>
              <a><i @click="showModalUser(false, 0)" class="pi pi-user-plus ml-2 cur-p"
                  style="color: rgb(33, 150, 243);"></i></a>
            </label>
            <MultiSelect :filter="true" v-model="publish_item.users" :options="all_users" optionValue="user_id"
              optionLabel="full_name" class="col-12 mt-2 ip36 p-0" placeholder="Người dùng" display="chip">
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Phòng ban nhận</b>
            </label>
            <TreeSelect class="col-12 ip36 mt-2" v-model="publish_item.departments" display="chip" selectionMode="checkbox" :options="category.departments" :showClear="true"
                    :max-height="200" placeholder="Chọn phòng ban">
            </TreeSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Thời gian
            </label>
            <InputText disabled v-model="publish_item.created_date_str" class="col-12 ip36" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Ý kiến/ Nội dung
            </label>
            <Textarea @change="changeMessageString(publish_item)" v-model="publish_item.message" spellcheck="false" class="col-12 ip36" autoResize autofocus
              style="padding:0.5rem;" />
          </div>
        </div>
        <div v-if="publish_item.message" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
                Xuất nội dung thành file XML
            </label>
            <InputSwitch v-model="publish_item.is_exported_xml" class="col-12" />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('phanphat')" class="p-button-text" />
      <Button label="Lưu" icon="pi pi-check" @click="savePublishDoc()" />
    </template>
  </Dialog>
  <Dialog :modal="true" :header="headerFollowPerson" v-model:visible="displayFollowPerson" :autoZIndex="true"
    :style="{ width: '50vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Chủ trì</b>
              <a><i @click="showModalUser(false, 1)" class="pi pi-user-plus ml-2 cur-p"
                  style="color: rgb(33, 150, 243);"></i></a>
            </label>
            <MultiSelect v-if="!selectedDoc.is_drafted" @change="filterFollowPerson(1)" :filter="true" v-model="followperson_item.hosts"
              :options="all_users" optionValue="user_id" optionLabel="full_name" class="col-12 mt-2 ip36 p-0"
              placeholder="Chủ trì" display="chip">
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </MultiSelect>
             <Dropdown v-if="selectedDoc.is_drafted" @change="filterFollowPerson(1)" :filter="true" v-model="followperson_item.hosts"
              :options="all_users" optionValue="user_id" optionLabel="full_name" class="col-12 mt-2 ip36 p-0"
              placeholder="Chủ trì">
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Phối hợp (cá nhân)</b>
              <a><i @click="showModalUser(false, 2)" class="pi pi-user-plus ml-2 cur-p"
                  style="color: rgb(33, 150, 243);"></i></a>
            </label>
            <MultiSelect @change="filterFollowPerson(2)" :filter="true" v-model="followperson_item.trackers"
              :options="all_users" optionValue="user_id" optionLabel="full_name" class="col-12 mt-2 ip36 p-0"
              placeholder="Người phối hợp" display="chip">
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Phối hợp (nhóm)</b>
            </label>
            <MultiSelect @change="filterFollowPerson(3)" v-model="followperson_item.track_groups"
              :options="track_groups" optionLabel="role_group_name" optionValue="role_group_id"
              placeholder="Nhóm phối hợp" :filter="true" display="chip" class="col-12 mt-2 p-0 text-left">
              <template #option="slotProps">
                <div class="country-item flex" style="justify-content: space-between; align-items: center; width: 100%">
                  <div>{{ slotProps.option.role_group_name }}</div>
                  <AvatarGroup class="mt-2" v-if="slotProps.option.users">
                    <Avatar v-for="item in slotProps.option.users.slice(0, 3)" :key="item.role_group_user_id"
                      v-tooltip.left="item.full_name"
                      v-bind:label="item.avatar ? '' : item.full_name.split(' ').at(-1).substring(0, 1)"
                      v-bind:image="basedomainURL + item.avatar"
                      style="background-color: #2196f3; color: #ffffff; vertical-align: middle" class="mr-2"
                      size="small" shape="circle" />
                    <Avatar v-if="slotProps.option.users && slotProps.option.users.length > 3"
                      v-bind:label="'+' + (slotProps.option.users.length - 3).toString()" shape="circle" size="small"
                      style="background-color: #2196f3; color: #ffffff" />
                  </AvatarGroup>
                </div>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Phối hợp (phòng ban)</b>
            </label>
            <TreeSelect @change="filterFollowPerson(6)" class="col-12 ip36 mt-2" v-model="followperson_item.track_departments" display="chip" selectionMode="checkbox" :options="category.departments" :showClear="true"
                    :max-height="200" placeholder="Chọn phòng ban">
            </TreeSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Xem để biết (cá nhân)</b>
              <a><i @click="showModalUser(false, 3)" class="pi pi-user-plus ml-2 cur-p"
                  style="color: rgb(33, 150, 243);"></i></a>
            </label>
            <MultiSelect @change="filterFollowPerson(4)" :filter="true" v-model="followperson_item.seetoknow_users"
              :options="all_users" optionValue="user_id" optionLabel="full_name" class="col-12 mt-2 ip36 p-0"
              placeholder="Người xem để biết" display="chip">
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Xem để biết (nhóm)</b>
            </label>
            <MultiSelect @change="filterFollowPerson(5)" v-model="followperson_item.seetoknow_groups"
              :options="seetoknow_groups" optionLabel="role_group_name" optionValue="role_group_id"
              placeholder="Nhóm xem để biết" :filter="true" display="chip" class="col-12 mt-2 p-0 text-left">
              <template #option="slotProps">
                <div class="country-item flex" style="justify-content: space-between; align-items: center; width: 100%">
                  <div>{{ slotProps.option.role_group_name }}</div>
                  <AvatarGroup class="mt-2" v-if="slotProps.option.users">
                    <Avatar v-for="item in slotProps.option.users.slice(0, 3)" :key="item.role_group_user_id"
                      v-tooltip.left="item.full_name"
                      v-bind:label="item.avatar ? '' : item.full_name.split(' ').at(-1).substring(0, 1)"
                      v-bind:image="basedomainURL + item.avatar"
                      style="background-color: #2196f3; color: #ffffff; vertical-align: middle" class="mr-2"
                      size="small" shape="circle" />
                    <Avatar v-if="slotProps.option.users && slotProps.option.users.length > 3"
                      v-bind:label="'+' + (slotProps.option.users.length - 3).toString()" shape="circle" size="small"
                      style="background-color: #2196f3; color: #ffffff" />
                  </AvatarGroup>
                </div>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Thời gian
            </label>
            <InputText disabled v-model="followperson_item.created_date_str" class="col-12 ip36" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Ý kiến/ Nội dung
            </label>
            <Textarea @change="changeMessageString(followperson_item)" v-model="followperson_item.message" spellcheck="false" class="col-12 ip36" autoResize autofocus
              style="padding:0.5rem;" />
          </div>
        </div>
        <div v-if="followperson_item.message" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
                Xuất nội dung thành file XML
            </label>
            <InputSwitch v-model="followperson_item.is_exported_xml" class="col-12" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Hạn xử lý
            </label>
            <InputSwitch v-model="followperson_item.is_deadline" class="col-8" />
          </div>
          <div v-if="followperson_item.is_deadline" class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 text-left flex" style="align-items:center;">
              Ngày xử lý
            </label>
            <Calendar class="col-8 p-0" id="date_publish" v-model="followperson_item.deadline_date" :manualInput="true"
              :showIcon="true" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-2 p-0 mt-3 text-left">File đính kèm</label>
            <div class="col-10 p-0">
              <FileUpload chooseLabel="Chọn File" :showUploadButton="false" :showCancelButton="false" :multiple="true"
                :maxFileSize="10000000000" @select="onUploadFile" @remove="removeFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll" :scrollable="true">
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2"
                            :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                        </div>
                      </template>
                      <template #end>
                        <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                          @click="deleteDocRelFile(slotProps.data)" />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('duyetcanhan')" class="p-button-text" />
      <Button label="Lưu" icon="pi pi-check" @click="saveFollowPerson()" />
    </template>
  </Dialog>
  <Dialog :modal="true" :header="headerFollowGroup" v-model:visible="displayFollowGroup" :autoZIndex="true"
    :style="{ width: '50vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Nhóm nhận</b>
            </label>
            <Dropdown @change="filterFollowGroup(1)" v-model="followgroup_item.receive_by" :options="follow_groups" optionLabel="role_group_name"
              optionValue="role_group_id" placeholder="Nhóm" :filter="true" display="chip"
              class="col-12 mt-2 p-0 text-left">
              <template #option="slotProps">
                <div class="country-item flex" style="justify-content: space-between; align-items: center; width: 100%">
                  <div>{{ slotProps.option.role_group_name }} <span v-if="slotProps.option.is_bydepartment">(Duyệt theo phòng ban)</span></div>
                  <AvatarGroup class="mt-2" v-if="slotProps.option.users">
                    <Avatar v-tooltip.top="item.full_name" v-for="item in slotProps.option.users.slice(0, 3)"
                      :key="item.role_group_user_id" v-tooltip.left="item.full_name"
                      v-bind:label="item.avatar ? '' : item.full_name.split(' ').at(-1).substring(0, 1)"
                      v-bind:image="basedomainURL + item.avatar"
                      style="background-color: #2196f3; color: #ffffff; vertical-align: middle" class="mr-2"
                      size="small" shape="circle" />
                    <Avatar v-if="slotProps.option.users && slotProps.option.users.length > 3"
                      v-bind:label="'+' + (slotProps.option.users.length - 3).toString()" shape="circle" size="small"
                      style="background-color: #2196f3; color: #ffffff" />
                  </AvatarGroup>
                </div>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Phối hợp (cá nhân)</b>
              <a><i @click="showModalUser(false, 5)" class="pi pi-user-plus ml-2 cur-p"
                  style="color: rgb(33, 150, 243);"></i></a>
            </label>
            <MultiSelect @change="filterFollowGroup(2)" :filter="true" v-model="followgroup_item.trackers"
              :options="all_users" optionValue="user_id" optionLabel="full_name" class="col-12 mt-2 ip36 p-0"
              placeholder="Người phối hợp" display="chip">
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Thời gian
            </label>
            <InputText disabled v-model="followgroup_item.created_date_str" class="col-12 ip36" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Ý kiến/ Nội dung
            </label>
            <Textarea @change="changeMessageString(followgroup_item)" v-model="followgroup_item.message" spellcheck="false" class="col-12 ip36" autoResize autofocus
              style="padding:0.5rem;" />
          </div>
        </div>
        <div v-if="followgroup_item.message" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
                Xuất nội dung thành file XML
            </label>
            <InputSwitch v-model="followgroup_item.is_exported_xml" class="col-12" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Hạn xử lý
            </label>
            <InputSwitch v-model="followgroup_item.is_deadline" class="col-8" />
          </div>
          <div v-if="followgroup_item.is_deadline" class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 text-left flex" style="align-items:center;">
              Ngày xử lý
            </label>
            <Calendar class="col-8 p-0" id="date_publish" v-model="followgroup_item.deadline_date" :manualInput="true"
              :showIcon="true" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-2 p-0 mt-3 text-left">File đính kèm</label>
            <div class="col-10 p-0">
              <FileUpload chooseLabel="Chọn File" :showUploadButton="false" :showCancelButton="false" :multiple="true"
                :maxFileSize="10000000000" @select="onUploadFile" @remove="removeFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll" :scrollable="true">
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2"
                            :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                        </div>
                      </template>
                      <template #end>
                        <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                          @click="deleteDocRelFile(slotProps.data)" />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('duyetnhom')" class="p-button-text" />
      <Button label="Lưu" icon="pi pi-check" @click="saveFollowGroup()" />
    </template>
  </Dialog>
  <Dialog :modal="true" :header="headerFollowDepartment" v-model:visible="displayFollowDepartment" :autoZIndex="true"
    :style="{ width: '50vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Phòng ban xử lý</b>
            </label>
            <TreeSelect @change="filterFollowDepartment(1)" class="col-12 ip36 mt-2" v-model="followdepartment_item.main_departments" display="chip" :selectionMode="selectedDoc.is_drafted ? 'single' : 'checkbox'" :options="category.departments" :showClear="true"
                    :max-height="200" placeholder="Chọn đơn vị">
            </TreeSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Phối hợp (phòng ban)</b>
            </label>
            <TreeSelect @change="filterFollowDepartment(2)" class="col-12 ip36 mt-2" v-model="followdepartment_item.track_departments" display="chip" selectionMode="checkbox" :options="category.departments" :showClear="true"
                    :max-height="200" placeholder="Chọn đơn vị">
            </TreeSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Phối hợp (cá nhân)</b>
              <a><i @click="showModalUser(false, 6)" class="pi pi-user-plus ml-2 cur-p"
                  style="color: rgb(33, 150, 243);"></i></a>
            </label>
            <MultiSelect @change="filterFollowDepartment(3)" :filter="true" v-model="followdepartment_item.trackers"
              :options="all_users" optionValue="user_id" optionLabel="full_name" class="col-12 mt-2 ip36 p-0"
              placeholder="Người phối hợp" display="chip">
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Thời gian
            </label>
            <InputText disabled v-model="followdepartment_item.created_date_str" class="col-12 ip36" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Ý kiến/ Nội dung
            </label>
            <Textarea @change="changeMessageString(followdepartment_item)" v-model="followdepartment_item.message" spellcheck="false" class="col-12 ip36" autoResize autofocus
              style="padding:0.5rem;" />
          </div>
        </div>
        <div v-if="followdepartment_item.message" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
                Xuất nội dung thành file XML
            </label>
            <InputSwitch v-model="followdepartment_item.is_exported_xml" class="col-12" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Hạn xử lý
            </label>
            <InputSwitch v-model="followdepartment_item.is_deadline" class="col-8" />
          </div>
          <div v-if="followdepartment_item.is_deadline" class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 text-left flex" style="align-items:center;">
              Ngày xử lý
            </label>
            <Calendar class="col-8 p-0" id="date_publish" v-model="followdepartment_item.deadline_date" :manualInput="true"
              :showIcon="true" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-2 p-0 mt-3 text-left">File đính kèm</label>
            <div class="col-10 p-0">
              <FileUpload chooseLabel="Chọn File" :showUploadButton="false" :showCancelButton="false" :multiple="true"
                :maxFileSize="10000000000" @select="onUploadFile" @remove="removeFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll" :scrollable="true">
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2"
                            :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                        </div>
                      </template>
                      <template #end>
                        <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                          @click="deleteDocRelFile(slotProps.data)" />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('duyetphongban')" class="p-button-text" />
      <Button label="Lưu" icon="pi pi-check" @click="saveFollowDepartment()" />
    </template>
  </Dialog>
  <Dialog :modal="true" :header="headerTransferStamp" v-model:visible="displayTransferStamp" :autoZIndex="true"
    :style="{ width: '50vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Người đóng dấu</b>
              <a><i @click="showModalUser(false, 7)" class="pi pi-user-plus ml-2 cur-p"
                  style="color: rgb(33, 150, 243);"></i></a>
            </label>
            <Dropdown :filter="true" v-model="transferstamp_item.user_id" :options="all_users" optionLabel="full_name"
              optionValue="user_id" placeholder="Người đóng dấu" spellcheck="false" class="col-12 mt-2 ip36 p-0">
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Thời gian
            </label>
            <InputText disabled v-model="transferstamp_item.created_date_str" class="col-12 ip36" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Ý kiến/ Nội dung
            </label>
            <Textarea @change="changeMessageString(transferstamp_item)" v-model="transferstamp_item.message" spellcheck="false" class="col-12 ip36" autoResize autofocus
              style="padding:0.5rem;" />
          </div>
        </div>
        <div v-if="transferstamp_item.message" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
                Xuất nội dung thành file XML
            </label>
            <InputSwitch v-model="transferstamp_item.is_exported_xml" class="col-12" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Hạn xử lý
            </label>
            <InputSwitch v-model="transferstamp_item.is_deadline" class="col-8" />
          </div>
          <div v-if="transferstamp_item.is_deadline" class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 text-left flex" style="align-items:center;">
              Ngày xử lý
            </label>
            <Calendar class="col-8 p-0" id="date_publish" v-model="transferstamp_item.deadline_date" :manualInput="true"
              :showIcon="true" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-2 p-0 mt-3 text-left">File đính kèm</label>
            <div class="col-10 p-0">
              <FileUpload chooseLabel="Chọn File" :showUploadButton="false" :showCancelButton="false" :multiple="true"
                :maxFileSize="10000000000" @select="onUploadFile" @remove="removeFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll" :scrollable="true">
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2"
                            :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                        </div>
                      </template>
                      <template #end>
                        <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                          @click="deleteDocRelFile(slotProps.data)" />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('chuyendongdau')" class="p-button-text" />
      <Button label="Lưu" icon="pi pi-check" @click="saveTransferStamp()" />
    </template>
  </Dialog>
  <Dialog class="black-text" :modal="true" :header="headerStamp" v-model:visible="displayStamp" :autoZIndex="true"
    :style="{ width: '90vw' }">
    <TabView v-model:activeIndex="activeTabStamp">
      <TabPanel>
        <template #header>
          <span>Cập nhật thông tin quản lý</span>
        </template>
        <form>
          <div class="grid formgrid m-2">
            <div class="field col-12 md:col-12 flex">
              <div class="col-8 md:col-8 m-0 p-0">
                 <div class="col-12 md:col-12 m-0 p-0 flex">
              <label class="col-3 p-0 text-left flex" style="align-items:center;">
                Văn bản của phòng ban
              </label>
              <InputSwitch @change="changeIsByDepartment" v-model="doc.is_by_department" class="col-9" />
              </div>
              <div v-if="!doc.is_by_department" class="field col-12 md:col-12 p-0 flex mt-3">
                <label class="col-3 p-0 text-left flex" style="align-items:center;">
                  Đơn vị
                  <span class="redsao pl-1"> (*)</span>
                </label>
                <TreeSelect @change="generateDocCode" class="col-8 p-0" v-model="doc.organization_id" :options="treeorgs"
                  :showClear="true" :max-height="200" placeholder="Chọn đơn vị">
                </TreeSelect>
                <!-- <Dropdown class="col-8 p-0" spellcheck="false" v-model="doc.organization_id" :options="treeorgs"
                            optionLabel="organization_name" optionValue="organization_id" :editable="false" :filter="true" /> -->
              </div>
              <div v-if="doc.is_by_department" class="field col-12 md:col-12 m-0 p-0 flex mt-3">
                <label class="col-3 p-0 text-left flex" style="align-items:center;">
                  Phòng ban
                </label>
                <TreeSelectCustom @change="generateDocCode" class="col-8 p-0" :removeTree="removeTreeSelect" :model="doc" property-name="department_id" :options="category.departments"
                placeholder="Chọn phòng ban">
              </TreeSelectCustom>
            </div>
                <div class="col-12 md:col-12 m-0 p-0 flex mt-3">
                  <label class="col-3 p-0 text-left flex" style="align-items:center;">
                    Vào số tự động
                  </label>
                  <InputSwitch @change="changeIsAutoNum" v-model="doc.is_auto_num" class="col-9" />
                </div>
              </div>
              <div class="col-4 md:col-4 p-0 m-0 flex">
                <button type="button" class="btn"
                  style="border: none; padding-left:10px;height:100%;width:100%;background-color:#ccebff;cursor: default">{{ doc_type_title }}</button>
              </div>
            </div>
            <div class="field col-12 md:col-12 flex">
              <div class="col-6 md:col-6 m-0 p-0 flex">
                <label class="col-4 p-0 text-left flex" style="align-items:center;">
                  Khối cơ quan
                </label>
                <Dropdown class="col-7 p-0" spellcheck="false" v-model="doc.dispatch_book_id"
                @change="generateDocCode(); changeIsAutoNum()"
                  :options="category.dispatch_books.filter(x => x.nav_type === doc.nav_type)"
                  optionLabel="dispatch_book_name" optionValue="dispatch_book_id" :editable="false" :filter="true" />
              </div>
              <div class="col-6 md:col-6 m-0 p-0 flex">
                <label class="col-4 text-left flex" style="align-items:center;">
                  Số vào sổ
                  <span class="redsao pl-1"> (*)</span>
                </label>
                <InputNumber @change="generateDocCode" :disabled="doc.is_auto_num" v-model="doc.dispatch_book_code" class="col-8 p-0 ip36"
                  :class="{ 'p-invalid': v$_stamp.dispatch_book_code.$invalid && submitted }" :useGrouping="false" />
              </div>
            </div>
            <div class="field col-12 md:col-12 flex">
              <div class="col-6 md:col-6 p-0 m-0 flex"></div>
              <div class="col-6 md:col-6 p-0 m-0 flex"
                v-if="(v$_stamp.dispatch_book_code.$invalid && submitted) || v$_stamp.dispatch_book_code.$pending.$response">
                <div class="col-4 text-left flex"></div>
                <small class="col-8 p-0">
                  <span style="color:red" class="col-12 ">{{
                      v$_stamp.dispatch_book_code.required.$message
                        .replace("Value", "Số vào sổ")
                        .replace("is required", "không được để trống")
                  }}</span>
                </small>
              </div>
            </div>
            <div class="field col-12 md:col-12 flex">
              <div class="col-6 md:col-6 m-0 p-0 flex">
                <label class="col-4 p-0 text-left flex" style="align-items:center;">
                  Số/Ký hiệu
                  <!-- <span class="redsao pl-1"> (*)</span> -->
                </label>
                <div class="p-inputgroup col-7 ip36 p-0">
                    <InputText @change="changeDocCode(); cancelReservationNumber()" v-model="doc.doc_code"
                     />
                    <Button @click="openModalReservationNumber" v-tooltip.right="'Lấy từ danh sách giữ số'" icon="pi pi-list"/>
                </div>
              </div>
              <div class="col-6 md:col-6 m-0 p-0 flex">
                <label class="col-4 text-left flex" style="align-items:center;">
              Ngày văn bản
              <!-- <span class="redsao pl-1"> (*)</span> -->
            </label>
            <Calendar @date-select="changeDocDate" class="col-8 p-0" v-model="doc.doc_date" :manualInput="true"
                  :showIcon="true" />
              </div>
            </div>
            <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Trích yếu
              <span class="redsao pl-1"> (*)</span>
            </label>
            <Textarea v-model="doc.compendium" spellcheck="false" class="col-10 ip36" autoResize autofocus
              :class="{ 'p-invalid': v$_stamp.compendium.$invalid && submitted }" style="padding:0.5rem;" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex"
            v-if="(v$_stamp.compendium.$invalid && submitted) || v$_stamp.compendium.$pending.$response">
            <div class="col-2 text-left flex"></div>
            <small class="col-10 p-0">
              <span style="color:red" class="col-12 ">{{
                  v$_stamp.compendium.required.$message
                    .replace("Value", "Trích yếu")
                    .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>
            <!-- <div class="field col-12 md:col-12 flex">
              <div class="col-6 md:col-6 p-0 m-0 flex"
                v-if="(v$_stamp.doc_code.$invalid && submitted) || v$_stamp.doc_code.$pending.$response">
                <div class="col-4 text-left flex"></div>
                <small class="col-8 p-0">
                  <span style="color:red" class="col-12 ">{{
                      v$_stamp.doc_code.required.$message
                        .replace("Value", "Số ký hiệu văn bản")
                        .replace("is required", "không được để trống")
                  }}</span>
                </small>
              </div>
              <div class="col-6 md:col-6 p-0 m-0 flex"
            v-if="((v$_stamp.doc_date.$invalid && submitted) || v$_stamp.doc_date.$pending.$response)">
            <div class="col-4 p-0 text-left flex"></div>
            <small class="col-8 p-0">
              <span style="color:red" class="col-12 ">{{
                  v$_stamp.doc_date.required.$message
                    .replace("Value", "Ngày văn bản")
                    .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
            </div> -->
            <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <div class="field col-12 md:col-12 m-0 p-0 flex">
              <div class="col-7 md:col-7 m-0 p-0 flex">
                <label class="col-7 m-0 p-0 text-left flex" style="align-items:center;margin-right: -3px !important;">
                  Số lượng bản
                </label>
                <InputNumber v-model="doc.num_of_copies" class="col-4 p-0 ip36" :useGrouping="false" />
              </div>
              <div class="col-5 md:col-5 m-0 p-0 flex">
                <label class="col-4 m-0 p-0 text-left flex" style="align-items:center;">
                  Số trang
                </label>
                <InputNumber v-model="doc.num_of_pages" style="padding-right: 5px !important;" class="col-6 p-0 ip36" :useGrouping="false" />
              </div>
            </div>
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <div class="field col-12 md:col-12 m-0 p-0 flex">
              <div class="col-7 md:col-7 m-0 flex">
                <label class="col-7 m-0 p-0 text-left flex" style="align-items:center;">
                  Độ mật
                </label>
                <Dropdown :showClear="true" class="col-5 p-0" spellcheck="false" v-model="doc.security" :options="category.security"
              optionLabel="security_name" optionValue="security_name" :editable="false" :filter="true" />
              </div>
              <div class="col-5 md:col-5 m-0 p-0 pl-2 flex">
                <label class="col-5 m-0 p-0 text-left flex" style="align-items:center;">
                  Độ khẩn
                </label>
                <Dropdown :showClear="true" class="col-7 p-0" spellcheck="false" v-model="doc.urgency" :options="category.urgency"
              optionLabel="urgency_name" optionValue="urgency_name" :editable="false" :filter="true" />
              </div>
            </div>
          </div>
        </div>
            <!-- <div class="field col-12 md:col-12 flex">
              <div class="col-6 md:col-6 p-0 m-0 flex"
                v-if="(v$_stamp.doc_date.$invalid && submitted) || v$_stamp.doc_date.$pending.$response">
                <div class="col-4 p-0 text-left flex"></div>
                <small class="col-8 p-0">
                  <span style="color:red" class="col-12 ">{{
                      v$_stamp.doc_date.required.$message
                        .replace("Value", "Ngày văn bản")
                        .replace("is required", "không được để trống")
                  }}</span>
                </small>
              </div>
              <div class="col-6 md:col-6 p-0 m-0 flex"></div>
            </div> -->
            <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Nhóm văn bản
            </label>
            <Dropdown :showClear="true" @change="changeDocGroup(); generateDocCode()" class="col-7 p-0" spellcheck="false" v-model="doc.doc_group_id"
              :options="category.groups" optionLabel="doc_group_name" optionValue="doc_group_id" :editable="false"
              :filter="true" />
          
          </div>
          <div class="col-6 md:col-6 p-0 m-0 flex">
            <label class="col-4 text-left flex" style="align-items:center;">
                  Không gửi bản giấy
                </label>
                <InputSwitch v-model="doc.is_not_send_paper" class="col-8" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Người ký
            </label>
                  <Dropdown @change="changeSigner($event); changeDocSigner()" :filter="true" v-model="doc.signer" :editable="true"
              :options="all_users" optionValue="full_name" optionLabel="full_name" class="col-7 mt-2 p-0"
              >
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </Dropdown>
            <!-- <InputText v-model="doc.signer" class="col-7 ip36" /> -->
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 text-left flex p-0" style="align-items:center;">
              Ngày hạn xử lý
            </label>
            <Calendar :showOnFocus="false" @input="autoFillDate(doc,'deadline_date')" id="deadline_date" class="col-8 p-0" v-model="doc.deadline_date" :manualInput="true"
              :showIcon="true" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Lãnh đạo
            </label>
            <InputText v-model="doc.ldt" class="col-7 ip36" />
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Chức vụ người ký
            </label>
            <Dropdown class="col-8 p-0" spellcheck="false" v-model="doc.position" :options="category.positions"
              optionLabel="position_name" optionValue="position_name" :editable="true" :filter="true" />
          </div>
        </div>
        <div v-if="doc.nav_type !== 1" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Cơ quan nhận
            </label>
            <Dropdown class="col-10 p-0" spellcheck="false" v-model="doc.receive_place" :options="category.receive_places"
              optionLabel="receive_place_name" optionValue="receive_place_name" :editable="true" :filter="true" />
          </div>
        </div>
        <div v-if="doc.nav_type !== 1" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Đơn vị liên quan
            </label>
            <InputText v-model="doc.related_unit" class="col-10 ip36" />
          </div>
        </div>
        <div v-if="doc.nav_type !== 1" class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Đơn vị soạn lưu
            </label>
            <InputText v-model="doc.composing_unit" class="col-7 ip36" />
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <div class="field col-12 md:col-12 m-0 p-0 flex">
              <div class="col-6 md:col-6 m-0 p-0 flex">
                <label class="col-6 m-0 p-0 text-left flex" style="align-items:center;">
                  Số bản lưu
                </label>
                <InputNumber v-model="doc.num_of_saved" class="col-6 p-0 ip36" :useGrouping="false" />
              </div>
              <div class="col-6 md:col-6 m-0 p-0 pl-2 flex">
                <label class="col-6 m-0 p-0 text-left flex" style="align-items:center;">
                  Số bản PH
                </label>
                <InputNumber v-model="doc.num_of_issue" class="col-6 p-0 ip36" :useGrouping="false" />
              </div>
            </div>
          </div>
        </div>
        <div v-if="doc.nav_type !== 1" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 p-0 m-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Người soạn thảo
            </label>
            <InputText v-model="doc.drafter" class="col-10 ip36" />
          </div>
        </div>
        <div v-if="doc.nav_type !== 1" class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Thời gian tổ chức
            </label>
            <Calendar :showOnFocus="false" :manualInput="true" :showIcon="true" class="col-7 p-0" v-model="doc.hold_time" :showTime="true" />
          </div>
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Địa điểm tổ chức
            </label>
            <InputText v-model="doc.hold_place" class="col-8" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Hình thức gửi
            </label>
            <Dropdown class="col-10 p-0" spellcheck="false" v-model="doc.send_way" :options="category.send_ways"
              optionLabel="send_way_name" optionValue="send_way_name" :editable="false" :filter="true" />
          </div>
        </div>
            <div v-if="!isEditStampDoc" class="col-12 md:col-12">
              <div class="field col-12 md:col-12 p-0 flex">
                <label class="col-2 p-0 mt-3 text-left">Văn bản dự thảo (gốc)<span class="redsao pl-1">
                    (*)</span></label>
                <div class="col-10 p-0 item-file-law">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2" :src="basedomainURL + '/Portals/Image/file/' + doc.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ doc.file_name }}</span>
                        </div>
                      </template>
                    </Toolbar>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="field col-12 md:col-12 p-0 flex">
                <label class="col-2 p-0 mt-3 text-left">Văn bản đính kèm (file số hoá)</label>
                <div class="col-10 p-0">
                  <FileUpload chooseLabel="Chọn File" accept=".pdf,.png, .jpg, .jpeg" :showUploadButton="false"
                    :showCancelButton="false" :multiple="false" :maxFileSize="10000000000" :fileLimit="1"
                    @select="onUploadOneFile" @remove="removeOneFile" />
                </div>
              </div>
            </div>
            <div v-if="isEditStampDoc && doc.file_path" class="col-12 md:col-12">
              <div class="field col-12 md:col-12 p-0 flex">
                <label class="col-2 p-0 mt-3 text-left"></label>
                <div class="col-10 p-0 item-file-law">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2" :src="basedomainURL + '/Portals/Image/file/' + doc.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ doc.file_name }}</span>
                        </div>
                      </template>
                      <template #end>
                        <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                          @click="deleteDocMainFile()" />
                      </template>
                    </Toolbar>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="field col-12 md:col-12 p-0 flex">
                <label class="col-2 p-0 mt-3 text-left">Tài liệu đính kèm khác</label>
                <div class="col-10 p-0">
                  <FileUpload chooseLabel="Chọn File" :showUploadButton="false" :showCancelButton="false"
                    :multiple="true" :maxFileSize="10000000000" @select="onUploadFile" @remove="removeFile" />
                </div>
              </div>
              <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
                <label class="mt-3 col-2 text-left"></label>
                <div class="col-10 p-0 item-file-law">
                  <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                    :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll"
                    :scrollable="true">
                    <template #list="slotProps">
                      <div class="w-full">
                        <Toolbar class="w-full">
                          <template #start>
                            <div class="flex align-items-center">
                              <img class="mr-2"
                                :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                                style="object-fit: contain;" width="40" height="40" />
                              <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                            </div>
                          </template>
                          <template #end>
                            <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                              @click="deleteDocRelFile(slotProps.data)" />
                          </template>
                        </Toolbar>
                      </div>
                    </template>
                  </DataView>
                </div>
              </div>
            </div>
            <div class="col-12 md:col-12">
              <Accordion :multiple="true">
                <AccordionTab header="THÔNG TIN KHÁC">
                  <div class="field col-12 md:col-12 p-0 flex">
                    <label class="col-2 mb-0 text-left flex" style="align-items:center;">Văn bản liên quan </label>
                    <MultiSelect v-model="doc.relate_doc" :options="allRelateDoc" optionLabel="compendium"
                      placeholder="Văn bản liên quan" display="chip" :filter="true"
                      class="multiselect-custom col-10 p-0">
                      <template #value="slotProps">
                        <div class="p-multiselect-car-token" v-for="option of slotProps.value"
                          :key="option.doc_master_id">
                          <div>{{ option.compendium + (option.doc_code ? (' (' + option.doc_code + ')') : '') }}</div>
                        </div>
                        <template v-if="!slotProps.value || slotProps.value.length === 0">
                          Văn bản liên quan
                        </template>
                      </template>
                      <template #option="slotProps">
                        <div class="country-item" style="overflow:hidden;white-space:normal;">
                          <div>{{ slotProps.option.compendium + (slotProps.option.doc_code ? (' (' +
                              slotProps.option.doc_code
                              + ')') : '')
                          }}</div>
                        </div>
                      </template>
                      <template #footer>
                        <div>
                          <Toolbar>
                            <template #end>
                              <div>
                                <Button icon="pi pi-trash" class="mr-2 p-button-danger" label="Xóa"
                                  @click="resetRelate">
                                </Button>
                              </div>
                            </template>
                          </Toolbar>
                        </div>
                      </template>
                    </MultiSelect>
                  </div>
                  <div class="col-12 field md:col-12 flex">
                    <div class="col-2 text-left pt-2 p-0" style="text-align: left">
                      Từ khoá
                    </div>
                    <Chips class="col-10 p-0 text-left" placeholder="Ấn Enter sau mỗi từ khóa" v-model="doc.key_tags" />
                  </div>
                  <!-- <div class="field col-12 md:col-12 flex">
                    <div class="col-6 md:col-6 m-0 p-0 flex">
                      <label class="col-4 text-left flex" style="align-items:center;">
                        Ngày có hiệu lực
                      </label>
                      <Calendar class="col-7 p-0" id="date_publish" v-model="doc.effective_date" :manualInput="true"
                        :showIcon="true" />
                    </div>
                    <div class="col-6 md:col-6 m-0 p-0 flex">
                      <label class="col-4 text-left flex" style="align-items:center;">
                        Ngày hết hiệu lực
                      </label>
                      <Calendar class="col-8 p-0" id="date_publish" v-model="doc.expiration_date" :manualInput="true"
                        :showIcon="true" />
                    </div>
                  </div> -->
                </AccordionTab>
                <AccordionTab header="THÔNG TIN LƯU TRỮ">
                  <div class="field col-12 md:col-12 flex">
                    <div class="col-6 md:col-6 m-0 p-0 flex">
                      <label class="col-4 p-0 text-left flex" style="align-items:center;">
                        Phòng/kho
                      </label>
                      <InputText v-model="doc.warehouse" class="col-7 ip36" />
                    </div>
                    <div class="col-6 md:col-6 m-0 p-0 flex">
                      <label class="col-4 p-0 text-left flex" style="align-items:center;">
                        Tủ
                      </label>
                      <InputText v-model="doc.cabinet" class="col-8 ip36" />
                    </div>
                  </div>
                  <div class="field col-12 md:col-12 flex">
                    <div class="col-6 md:col-6 m-0 p-0 flex">
                      <label class="col-4 p-0 text-left flex" style="align-items:center;">
                        Kệ
                      </label>
                      <InputText v-model="doc.shelf" class="col-7 ip36" />
                    </div>
                    <div class="col-6 md:col-6 m-0 p-0 flex">
                      <label class="col-4 p-0 text-left flex" style="align-items:center;">
                        Hộp
                      </label>
                      <InputText v-model="doc.box" class="col-8 ip36" />
                    </div>
                  </div>
                  <div class="field col-12 md:col-12 flex">
                    <div class="col-12 md:col-12 p-0 m-0 flex">
                      <label class="col-2 p-0 text-left flex" style="align-items:center;">
                        Ghi chú
                      </label>
                      <Textarea v-model="doc.notes" spellcheck="false" class="col-10 ip36" autoResize autofocus
                        style="padding:0.5rem;" />
                    </div>
                  </div>
                </AccordionTab>
              </Accordion>
            </div>
          </div>
        </form>
      </TabPanel>
      <TabPanel v-if="doc.is_approved">
        <template #header>
          <span>Đóng dấu và vào sổ</span>
        </template>
        <div v-if="isLoaded">
          <DocSignIframe :returnWatermark="returnWatermark" :Type="'stamp'" :DocObj="doc" :listFileUploaded="listFileUploaded" />
        </div>
      </TabPanel>
    </TabView>
    <template #footer>
      <div class="flex mt-3" style="justify-content: space-between">
        <Button label="Hủy" icon="pi pi-times" @click="closeDialog('dongdau')" class="p-button-text" />
        <div v-if="doc.is_approved" style="color:#f44336; text-align: left; font-weight: 500">
          <div style="font-style: italic">Bước 1: Click chọn chữ ký muốn chèn vào văn bản, Bước 2: Chọn vị trí trong văn
            bản cần chèn và click chuột phải</div>
          <div style="font-style: italic">Chú ý: Không Zoom văn bản trước khi ký</div>
        </div>
        <Button label="Lưu" icon="pi pi-check" @click="getWatermark('stamp',!v$_stamp.$invalid)" />
      </div>
    </template>
  </Dialog>
  <Dialog :modal="true" :header="headerCompleted" v-model:visible="displayCompleted" :autoZIndex="true"
    :style="{ width: '50vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Nội dung
            </label>
            <Textarea v-model="completed_item.message" spellcheck="false" class="col-12 ip36" autoResize autofocus
              style="padding:0.5rem;" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-2 p-0 mt-3 text-left">File đính kèm</label>
            <div class="col-10 p-0">
              <FileUpload chooseLabel="Chọn File" :showUploadButton="false" :showCancelButton="false" :multiple="true"
                :maxFileSize="10000000000" @select="onUploadFile" @remove="removeFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll" :scrollable="true">
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2"
                            :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                        </div>
                      </template>
                      <template #end>
                        <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                          @click="deleteDocRelFile(slotProps.data)" />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('xacnhanhoanthanh')" class="p-button-text" />
      <Button label="Lưu" icon="pi pi-check" @click="saveCompleted()" />
    </template>
  </Dialog>
  <Dialog :modal="true" :header="headerReturn" v-model:visible="displayReturn" :autoZIndex="true"
    :style="{ width: '50vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Thời gian
            </label>
            <InputText disabled v-model="return_item.created_date_str" class="col-12 ip36" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Nội dung
            </label>
            <Textarea @change="changeMessageString(return_item)" v-model="return_item.message" spellcheck="false" class="col-12 ip36" autoResize autofocus
              style="padding:0.5rem;" />
          </div>
        </div>
        <div v-if="return_item.message" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0 flex">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
                Xuất nội dung thành file XML
            </label>
            <InputSwitch v-model="return_item.is_exported_xml" class="col-12" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-2 p-0 mt-3 text-left">File đính kèm</label>
            <div class="col-10 p-0">
              <FileUpload chooseLabel="Chọn File" :showUploadButton="false" :showCancelButton="false" :multiple="true"
                :maxFileSize="10000000000" @select="onUploadFile" @remove="removeFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll" :scrollable="true">
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2"
                            :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                        </div>
                      </template>
                      <template #end>
                        <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                          @click="deleteDocRelFile(slotProps.data)" />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('tralai')" class="p-button-text" />
      <Button label="Lưu" icon="pi pi-check" @click="saveReturn()" />
    </template>
  </Dialog>
   <Dialog :modal="true" :header="headerApproval" v-model:visible="displayApproval" :autoZIndex="true"
    :style="{ width: '90vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <DocSignIframe :returnWatermark="returnWatermark" v-if="isLoaded" :Type="'sign'" :DocObj="selectedDoc" :listFileUploaded="iframe_files" />
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              <b>Người nhận</b>
              <a><i @click="showModalUser(true, 4)" class="pi pi-user-plus ml-2 cur-p"
                  style="color: rgb(33, 150, 243);"></i></a>
            </label>
            <Dropdown :filter="true" v-model="approval_item.receive_by" :options="all_users" optionLabel="full_name"
              optionValue="user_key" placeholder="Người nhận" spellcheck="false" class="col-12 mt-2 ip36 p-0">
              <template #option="slotProps">
                <div class="country-item flex">
                  <Avatar :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/nouser1.png'
                  " class="mr-2 w-2rem h-2rem" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ slotProps.option.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ slotProps.option.organization_name }}</div>
                    <div style="color: #aaa; ">{{ slotProps.option.department_name }}</div>
                  </div>
                </div>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="col-12 md:col-12 field flex">
          <div class="field flex col-1">
              <label class="col-7 m-0 p-0 text-left flex" style="align-items:center;">
                Ưu tiên
              </label>
              <InputSwitch v-model="approval_item.is_prioritized" class="col-5" />
          </div>
          <div v-if="type_approval === 'trinhpheduyet'" class="field flex col-2">
              <label class="col-7 m-0 p-0 text-left flex" style="align-items:center;">
                Xác nhận ký nháy
              </label>
              <InputSwitch @change="changeTypeSignature(approval_item.flash_sign)" v-model="approval_item.flash_sign" class="col-5" />
          </div>
          <!-- <div v-if="type_approval === 'duyetphathanh' && selectedDoc.nav_type === 2" class="field flex col-2">
              <label class="col-7 m-0 p-0 text-left flex" style="align-items:center;">
                Ký điện tử (Sim CA)
              </label>
              <InputSwitch v-model="approval_item.esim_sign" class="col-5" />
          </div> -->
        </div>
        <div v-if="approval_item.esim_sign" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Lý do
            </label>
            <InputText v-model="approval_item.reason" class="col-12 mt-2 ip36" />
          </div>
        </div>
        <div v-if="approval_item.esim_sign" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Địa điểm
            </label>
            <InputText v-model="approval_item.place" class="col-12 mt-2 ip36" />
          </div>
        </div>
        <div v-if="approval_item.esim_sign" class="col-12 md:col-12 field flex">
          <div class="field flex col-2">
              <label class="col-6 m-0 p-0 text-left flex" style="align-items:center;">
                Hiển thị "ký bởi"
              </label>
              <InputSwitch v-model="approval_item.show_signby" class="col-6" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Thời gian
            </label>
            <InputText disabled v-model="approval_item.created_date_str" class="col-12 mt-2 ip36" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Nội dung
            </label>
            <Textarea @change="changeMessageString(approval_item)" v-model="approval_item.message" spellcheck="false" class="col-12 ip36 mt-2" autoResize
              style="padding:0.5rem;" />
          </div>
        </div>
        <div v-if="approval_item.message" class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
                Xuất nội dung thành file XML
            </label>
            <InputSwitch v-model="approval_item.is_exported_xml" class="col-12" />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-2 p-0 text-left flex" style="align-items:center;">
              Hạn xử lý
            </label>
            <InputSwitch v-model="approval_item.is_deadline" class="col-10" />
          </div>
          <div v-if="approval_item.is_deadline" class="col-6 md:col-6 m-0 p-0 flex">
            <label class="col-4 text-left flex" style="align-items:center;">
              Ngày xử lý
            </label>
            <Calendar class="col-8 p-0" id="deadline_date" v-model="approval_item.deadline_date" :manualInput="true"
              :showIcon="true" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex">
            <label class="col-1 p-0 mt-3 text-left">File đính kèm</label>
            <div class="col-11 p-0">
              <FileUpload chooseLabel="Chọn File" :showUploadButton="false" :showCancelButton="false" :multiple="true"
                :maxFileSize="10000000000" @select="onUploadFile" @remove="removeFile" />
            </div>
          </div>
          <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
            <label class="mt-3 col-2 text-left"></label>
            <div class="col-10 p-0 item-file-law">
              <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll" :scrollable="true">
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div class="flex align-items-center">
                          <img class="mr-2"
                            :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                            style="object-fit: contain;" width="40" height="40" />
                          <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                        </div>
                      </template>
                      <template #end>
                        <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                          @click="deleteDocRelFile(slotProps.data)" />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <div class="flex mt-3" style="justify-content: space-between">
        <Button label="Hủy" icon="pi pi-times" @click="closeDialog('pheduyet')" class="p-button-text" />
        <div style="color:#f44336; text-align: left; font-weight: 500">
            <div style="font-style: italic">Bước 1: Click chọn chữ ký muốn chèn vào văn bản, Bước 2: Chọn vị trí trong văn
              bản cần chèn và click chuột phải</div>
            <div style="font-style: italic">Chú ý: Không Zoom văn bản trước khi ký</div>
          </div>
        <Button label="Lưu" icon="pi pi-check" @click="getWatermark('sign')" />
      </div>
    </template>
  </Dialog>
   <Dialog :modal="true" :header="headerReservationNumber" v-model:visible="displayReservationNumber" style="z-index: 1000"
    :style="{ width: '70vw' }">
    <div>
          <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <div style="justify-content: space-between;" v-for="item in reservation_numbers" :key="item.reservation_number_id" class="code-item  flex">
             <div class="flex">
              <RadioButton :inputId="item.reservation_number_id" name="item" :value="item.reservation_number_id" v-model="selectedReservationNumber" class="mr-5" inputId="rb1" />
                <label :for="item.reservation_number_id"><h3 class="m-0">{{ item.reservation_code }}</h3></label>
             </div>
             <div>
              <Chip v-if="item.is_private" v-bind:label="'Chỉ mình tôi'" style="background-color:#2196F3;color: #fff; border-radius: 5px; font-size: 11px;" />
             </div>
            </div>
            <div
            class="
              align-items-center
              justify-content-center
              p-4
              text-center
              m-auto
            "
            v-if="reservation_numbers.length === 0"
          >
            <img src="../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
          </div>
        </div>
      </div>
    </form>
        </div>
        <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('laysogiu')" class="p-button-text" />
      <Button label="Chọn" icon="pi pi-check" @click="pickReservationNumber()" />
    </template>
  </Dialog>
  <DocShareFile v-if="displayAddStore" :displayAddStore ="displayAddStore" :typeShare="typeShare" :DocSelected_ID="DocSelected_ID" :Doc_Compendium="Doc_Compendium"/>
  <DocLinkTask v-if="displayDialogLinkTask === true" :headerDialog="headerDialogLinkTask" :id="selectedDoc.doc_master_id"
    :displayDialog="displayDialogLinkTask" :closeDialog="CloseDiaLogLinkTask" />
  <DocSelectIssuePlace v-if="displayDialogIssuePlace === true" :headerDialog="headerDialogIssuePlace" :array="same_issue_place"
    :displayDialog="displayDialogIssuePlace" :closeDialog="CloseDiaLogIssuePlace" :selectModel="selectIssuePlace" />
    <DocSelectReceivePlaceOnline v-if="displayDialogReceivePlaceOnline === true" :headerDialog="headerDialogReceivePlaceOnline" :array="same_receive_place_online"
    :displayDialog="displayDialogReceivePlaceOnline" :closeDialog="CloseDiaLogReceivePlaceOnline" :selectModel="selectReceivePlaceOnline" />
  <DocConnectSendDialog :checkedDocs="checked_docs" v-if="displayDocConnectSend" :key="componentKey" :displayDocConnectSend="displayDocConnectSend" :closeDialog="closeDialogDocConnectSend" />
  <treeuser
    v-if="displayDialogUser === true"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="is_one"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
</template>
<style lang='scss' scoped>
.wrap-multi.p-multiselect{
  height: auto; 
  min-height: 36px
}
::v-deep(.wrap-multi.p-multiselect) {
   .p-multiselect-label
  {
    height: 100%;
    display: flex;
    align-items: center;
    white-space: normal !important;
  }

  .p-chip-text {
    word-break: break-word !important;
  }

  .p-chip img {
    margin: 0;
  }
}
::v-deep(.wrap-multi.p-multiselect){
  .p-multiselect-label{
    flex-wrap: wrap;
    row-gap: 0.3rem;
  }
  .p-multiselect-token{
    max-width: 100%;
  }
  .p-multiselect-token-label{
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 1;
    -webkit-box-orient: vertical;
  }

}
.btn-save-continued:focus-visible{
  background: #689F38;
    color: #fff;
    border-color: #689F38;
}
.doc-wrap {
  height: 100%;
}

.doc-header {
  /* margin: 1rem; */
  border-top: 1px solid #e9ecef;
  padding: 0.5rem;
  margin-bottom: 0;
}

.doc-body {
  height: 100%;
  /* border-top: 1px solid #e9ecef; */
}

::v-deep(.p-splitbutton.p-button-outlined.p-button-secondary) {
  .p-button {
    background-color: transparent;
    color: #64748B;
    border: 1px solid;
  }

  .p-button.p-splitbutton-defaultbutton {
    border-right: 0 none;
  }

  .p-button:enabled:hover,
  .p-button:not(button):not(a):not(.p-disabled):hover {
    background: rgba(100, 116, 139, 0.04);
    color: #64748B;
    border-color: #475569;
  }

  .p-button:enabled:active,
  .p-button:not(button):not(a):not(.p-disabled):active {
    background: rgba(100, 116, 139, 0.16);
    color: #64748B;
    border-color: #475569;
  }
}
.code-item{
  padding-bottom: 10px;
  margin-bottom: 10px;
    border-bottom: 1px solid rgb(242,242,242);
}
</style>