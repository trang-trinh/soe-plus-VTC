<script setup>
import { onMounted, ref, inject } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import DocList from "../../components/doc/DocList.vue";
import DocFilter from "../../components/doc/DocFilter.vue";
import DocDetail from "../../components/doc/DocDetail.vue";
import DocOrgChart from "../../components/doc/DocOrgChart.vue";
import TreeSelectCustom from "../../components/doc/TreeSelectCustom.vue";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import treeuser from "../../components/user/treeuser.vue";
import { encr } from "../../util/function";
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
// Emit
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "goToDetailDoc":
      if (obj.data) {
        loadUserFunctions(obj.data);
        selectedDoc.value = obj.data;
      }
      break;
    case "choiceusers":
      displayDialogUser.value = obj.data["displayDialog"];
      if (obj.data["submit"]) {
        choiceUser();
      }
      break;
      case "returnWatermark":
    
      if (obj.data) {
        watermark.value = obj.data;
        saveStamp(!v$_stamp.value.$invalid);
      }
      break;
      case "returnRecallSelection":
      if (obj.data) {
        returnRecallSelection(obj.data);
      }
      break;
    default: break;
  }
});
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
      var notexist = selectedUser.value.filter(
        (a) =>
          followperson_item.value.hosts.findIndex(

            (b) => b === a["user_id"]
          ) === -1
      );
      followperson_item.value.hosts = followperson_item.value.hosts.concat(notexist.map(x => x.user_id));
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
    default:
      break;
  }
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
const v$_dt = useVuelidate(rules_duthao, doc);
const v$ = useVuelidate(rules, doc);
// utils
const convertDatefromDB = (obj) => {
  let listKeyDate = ["created_date","deadline_date","doc_date","effective_date","expiration_date","handle_date","receive_date","hold_time"];
  for (var key in obj) {
		let isnum = /^\d+$/.test(obj[key]);
		if (!isnum && moment(obj[key], moment.ISO_8601, true).isValid()) {
      //obj[key] = new Date(obj[key]);
			let idxDate = listKeyDate.indexOf(key);
			if (key in obj && idxDate >= 0) {
				obj[listKeyDate[idxDate]] = new Date(obj[listKeyDate[idxDate]]);
			}
		}
  }
}
// defined function doc
const loadUserFunctions = (docpar) => {
  axios
    .post(
      baseURL
      //baseUrl
      + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_master_store_functions_list",
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
       
        if(data[0].find(x=>x.function === 'xoakhovanban')){
          allowDelDoc.value = true;
          doc_del.value = selectedDoc.value;
        }
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
      category.value.issue_places = data[0];

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
          { par: "department_id", va: doc.value.department_id ? Object.keys(doc.value.department_id)[0] : null },
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
          sokh += data[0][i].value + data[0][i].separator;
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
const openModalSohoa = (type, data) => {
  isViewDoc.value = false;
  let title = type === 1 ? 'đến' : (type === 2 ? 'đi' : 'nội bộ');
  doc_type_title.value = "Văn bản " + title;
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
    isAddDoc.value = true;
    headerDoc.value = "Thêm mới văn bản " + title;
    doc.value = {
      receive_date: new Date(),
      doc_group: category.value.groups.length > 0 ? category.value.groups[0].doc_group_name : null,
      doc_group_id: category.value.groups.length > 0 ? category.value.groups[0].doc_group_id : null,
      organization_id: options.value.organization_id,
      is_auto_num: true,
      is_handle: false,
      is_drafted: false,
      is_inworkflow: false,
      created_by: store.getters.user.user_key,
      nav_type: type,
      urgency: category.value.urgency.length > 0 ? category.value.urgency[0].urgency_name : null,
      rel_docs: [],
      tags: ""
    };
    doc.value.dispatch_book_id = category.value.dispatch_books.filter(x => x.nav_type === doc.value.nav_type).length > 0 ? category.value.dispatch_books.filter(x => x.nav_type === doc.value.nav_type)[0].dispatch_book_id : null;
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
    if(doc.value.is_auto_num){
      changeIsAutoNum();
    }
    generateDocCode();
  }
  displayDocSohoa.value = true;
  submitted.value = false;
};
const openModalDuthao = (type, data) => {
  isViewDoc.value = false;
  let title = type === 2 ? 'đi' : 'nội bộ';
  doc_type_title.value = "Văn bản " + title;
  files = [];
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
      is_handle: false,
      is_inworkflow: false,
      created_by: store.getters.user.user_key,
      nav_type: type,
      drafter: store.getters.user.full_name,
      rel_docs: [],
      tags: "",
      is_by_department: true
    }
    // if(store.getters.user.role_id !== 'vanthu'){
    //   let keyobj = {};
    //   keyobj[store.getters.user.department_id] = true;
    //   doc.value.department_id = keyobj;
    // }
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
// change nguoi ky
const changeSigner = (us) => {
  doc.value.position = all_users.value.find(x=>x.full_name === us.value).position_name;
}
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
  if (doc.value.organization_id) {
    doc.value.organization_id = Object.keys(doc.value.organization_id)[0];
  }
  if (doc.value.department_id) {
    doc.value.department_id = Object.keys(doc.value.department_id)[0];
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
const saveDocDuthao = (isFormValid) => {
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
  if (doc.value.organization_id) {
    doc.value.organization_id = Object.keys(doc.value.organization_id)[0];
  }
  if (doc.value.department_id) {
    doc.value.department_id = Object.keys(doc.value.department_id)[0];
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
  submitted.value = true;
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
        //console.log(response.data.ms);
        swal.fire({
          title: "Thông báo",
          text: "Xảy ra lỗi khi cập nhật văn bản.",
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
}
const closeDialog = (type_doc, type) => {
  switch (type_doc) {
    case 'sohoa':
      displayDocSohoa.value = false;
      doc.value = {
        receive_date: new Date(),
        doc_group: category.value.groups.length > 0 ? category.value.groups[0].doc_group_name : null,
        organization_id: options.value.organization_id,
        is_auto_num: true,
        is_handle: false,
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
    default:
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
              //console.log(response.data.ms);
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
// View doc
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
onMounted(() => {
  //init
  loadUserFunctions();
  loadCategorys();
  loadOrgs();
  loadUsers();
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
          <DocFilter :Type="'store'"></DocFilter>
        </template>

        <template #end>
          <Button class="mr-2 p-button-outlined p-button-secondary" icon="pi pi-refresh" @click="onRefresh" />
          <Button label="Xoá" icon="pi pi-trash" class="mr-2 p-button-danger" @click="delDoc()" v-if="allowDelDoc" />
        </template>
      </Toolbar>
    </div>
    <div class="doc-body surface-0">
      <Splitter class="w-full">
        <SplitterPanel :size="35">
          <DocList :Type="'store'"/>
        </SplitterPanel>
        <SplitterPanel :size="65">
          <DocDetail :Type="'store'" :goToViewDoc="openModalViewDoc" :goToDetailSohoa="goToDetailSohoa" :goToDetailDuthao="goToDetailDuthao" :goToDetailStamp="goToDetailStamp"/>
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
            <Dropdown class="col-10 p-0" spellcheck="false" v-model="doc.issue_place" :options="category.issue_places"
              optionLabel="issue_place_name" optionValue="issue_place_name" :editable="true" :filter="true" />
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
  <treeuser v-if="displayDialogUser === true" :headerDialog="headerDialogUser" :displayDialog="displayDialogUser"
    :one="is_one" :selected="selectedUser" />
</template>
<style lang='scss' scoped>
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
</style>