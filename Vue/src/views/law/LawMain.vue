<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { VuemojiPicker } from "vuemoji-picker";
import moment from "moment";
//import commentLaws from "../../components/news/comment.vue";
import detailsDocLaws from "../../components/law/LawDetail.vue";
import { encr, checkURL } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");
//Nơi nhận EMIT từ component
const emitter = inject("emitter");
emitter.on("emitData", (obj) => {
	switch (obj.type) {
		case "loadListLaw":
			if (obj.data) {
				closeDialog('reload');
			}
			else {
				closeDialog();
				loadDataLaw(true);
				showDetailLaw(detailLaw.value);
			}
			break;
		default:
			break;
	}
});

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
]);
const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: 0,
  LawType: "",
  typeviews: 0,
  organization_id: store.getters.user.organization_id,
  user_key: store.getters.user.user_key,
});

const typeViewsCount = ref([
  { name: "Mới nhất", code: 0, value: "created_date" },
  { name: "Xem nhiều", code: 1, value: "times_view" },
  { name: "Tải nhiều", code: 2, value: "times_download" },
]);

const rules = {
  law_name: {
    required,
	maxLength: maxLength(1500),
    $errors: [
      {
        $property: "law_name",
        $validator: "required",
        $message: "Tên văn bản luật không được để trống!",
      },
    ],
  },
  law_number: {
    required,
	maxLength: maxLength(250),
    $errors: [
      {
        $property: "law_number",
        $validator: "required",
        $message: "Số hiệu văn bản không được để trống!",
      },
    ],
  },
};

const activeType = ref(0);
const allRelateLaw = ref([]);

const headerLaw = ref();
const displayLaw = ref(false);
const law = ref({
  law_name: "",
  law_number: null,
  summary: "",
  tags: "",
  is_order: 1,
  is_active: true,
  is_new: 1,
  publish_date: null,
  issued_date: null,
  expiration_date: null,
});
const submitted = ref(false);
const v$ = useVuelidate(rules, law);
//const issaveLaw = ref(false);
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const toast = useToast();
const datalists = ref([]);
const layout = ref("list");

//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};

const sttLaw = ref();

const isFirst = ref(true);
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
		options.value.id = datalists.value[datalists.value.length - 1].law_id;
		options.value.IsNext = true;
	} else if (event.page < options.value.PageNo) {
		//Trang trước
		options.value.id = datalists.value[0].law_id;
		options.value.IsNext = false;
	}
	options.value.PageNo = event.page;
	loadDataLaw(true);
};
// to component 4
const loadDataListByID = (dataLaw) => {
	axios
		.post(
			baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_documents_data_list",
						par: [
							{ par: "law_id", va: dataLaw.law_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			let objLaw = data[0][0];
			if (objLaw.publish_date) {
				objLaw.publish_date = new Date(objLaw.publish_date);
			}
			if (objLaw.issued_date) {
				objLaw.issued_date = new Date(objLaw.issued_date);
			}
			if (objLaw.expiration_date) {
				objLaw.expiration_date = new Date(objLaw.expiration_date);
			}
			if (objLaw.modified_date == null && objLaw.created_date) {
				objLaw.modified_date = objLaw.created_date;
			}
			if (objLaw.modified_date) {
				objLaw.modified_date = new Date(objLaw.modified_date);
			}
			if (objLaw.tags != null && objLaw.tags.length > 1) {
				if (!Array.isArray(objLaw.tags)) {
					objLaw.key_tags = objLaw.tags.split(",");
				}
			}
			if (objLaw.field_name != null && objLaw.field_name.length > 1) {
				if (!Array.isArray(objLaw.field_name)) {
					let listFields = objLaw.field_name.split(",");
					objLaw.key_fields = [];
					listFields.forEach((element, i) => {
						let field = { name: element.trim(), code: element.trim() };
						objLaw.key_fields.push(field);
					});				
				}
			}

			data[1].forEach((element, i) => {
				element.file_type = element.file_type.toLowerCase();
			});
			listFileUploaded.value = data[1];
			listFileDel.value = data[1];
			objLaw.relate_law = data[2];
			objLaw.replace_law = data[3];
			law.value = objLaw;
		})
		.catch((error) => {
			toast.error("Tải dữ liệu không thành công!");
			if (error && error.status === 401) {
				swal.fire({
					
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
// end component 4
const deleteFileLaw = (value) => {
	listFileUploaded.value = listFileUploaded.value.filter(x => x.file_id != value.file_id);
};
const listFileUploaded = ref([]); // danh sách file đã up của văn bản luật
const listFileDel = ref([]); // danh sách file bị xóa khi cập nhật của văn bản luật
const isAddLaw = ref(true);
const openLaw = (data) => {
	files = [];
	listFileUploaded.value = [];
	listFileDel.value = [];
	if (data != null && data.law_id != null) {
		isAddLaw.value = false;
		headerLaw.value = "Cập nhật văn bản luật";		
		loadDataListByID(data);
	}
	else {
		isAddLaw.value = true;
		headerLaw.value = "Thêm mới văn bản luật";
		law.value = {
			law_name: "",
			law_number: null,
			summary: "",
			tags: "",
			is_order: 1,
			is_active: true,
			is_new: 1,
			publish_date: null,
			issued_date: null,
			expiration_date: null
		};
	}
	if (store.state.user.is_super == true) {
		law.value.organization_id = 0;
	} else {
		law.value.organization_id = store.getters.user.organization_id;
	}
	displayLaw.value = true;
	submitted.value = false;
};
const closeDialog = (type) => {
  displayLaw.value = false;
  law.value = {
    law_name: "",
    law_number: null,
    summary: "",
    tags: "",
    is_order: 1,
    is_active: true,
    is_new: 1,
    publish_date: null,
    issued_date: null,
    expiration_date: null,
  };
  if (type != null) {
    loadDataLaw("first");
  }
};

const listTypes = ref([]);
const listTypesOpen = ref([]);
const loadTypesList = () => {
	listTypesOpen.value = [];
	listTypes.value = [];
	axios
		.post(
			baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_list_types",
						par: [
							{ par: "organization_id", va: store.getters.user.organization_id },
							{ par: "user_key", va: store.getters.user.user_key },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data)[0];
			listTypesOpen.value.push({
				name: "Tất cả",
				code: "",
			});
			data.forEach((element) => {
				if (element.law_type_name != null) {
					listTypes.value.push({
						name: element.law_type_name,
						code: element.law_type_name,
					});
					listTypesOpen.value.push({
						name: element.law_type_name,
						code: element.law_type_name,
					});
				}
			});
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

const listIssuePlaces = ref([]);
const loadIssuePlacesList = () => {
  listIssuePlaces.value = [];
  axios
    .post(
	  	baseUrlCheck + "api/law_documents/GetDataProc",
		{ 
			str: encr(JSON.stringify({
					proc: "law_list_issue_places",
					par: [
						{ par: "organization_id", va: store.getters.user.organization_id },
          				{ par: "user_key", va: store.getters.user.user_key },
					],
				}), SecretKey, cryoptojs
			).toString()
		},
		config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

			data.forEach((element) => {
				if (element.issue_place_name != null) {
					listIssuePlaces.value.push({
						name: element.issue_place_name,
						code: element.issue_place_name,
					});
				}
			});
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

const listFields = ref([]);
const loadFieldsList = () => {
  listFields.value = [];
  axios
    .post(
	  	baseUrlCheck + "api/law_documents/GetDataProc",
		{ 
			str: encr(JSON.stringify({
					proc: "law_list_fields",
					par: [
						{ par: "organization_id", va: store.getters.user.organization_id },
						{ par: "user_key", va: store.getters.user.user_key },
					],
				}), SecretKey, cryoptojs
			).toString()
		},
		config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

			data.forEach((element) => {
				if (element.field_name != null) {
					listFields.value.push({
						name: element.field_name,
						code: element.field_name,
					});
				}
			});
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

const listSigners = ref([]);
const loadSignersList = () => {
  listSigners.value = [];
  axios
    .post(
	  	baseUrlCheck + "api/law_documents/GetDataProc",
		{ 
			str: encr(JSON.stringify({
					proc: "law_list_signers",
					par: [
						{ par: "organization_id", va: store.getters.user.organization_id },
						{ par: "user_key", va: store.getters.user.user_key },
					],
				}), SecretKey, cryoptojs
			).toString()
		},
		config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

			data.forEach((element) => {
				if (element.signer_name != null) {
					listSigners.value.push({
						name: element.signer_name,
						code: element.signer_name,
					});
				}
			});
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

const loadAllDocsLaw = () => {
	axios
      	.post(
			baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_documents_list",
						par: [
							{ par: "organization_id", va: store.getters.user.organization_id },
							{ par: "user_id ", va: store.getters.user.user_id },
							{ par: "pageno", va: 0 },
							{ par: "pagesize", va: 0 },
							{ par: "search", va: null },
							{ par: "law_type", va: 0 },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
      )
      .then((response) => {
		let data = JSON.parse(response.data.data)[0];
		allRelateLaw.value = data;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        addLog({
          title: "Lỗi Console loadData",
          controller: "SQLView.vue",
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
const loadCountLaw = () => {
  axios
    .post(
	  	baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_documents_count",
						par: [
							{ par: "organization_id", va: store.getters.user.organization_id },
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "search", va: options.value.SearchText },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        sttLaw.value = options.value.totalRecords + 1;
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
const loadDataLaw = (rf) => {
  if (rf) {
    if (isDynamicSQL.value) {
      loadDataSQL();
      return false;
    }
    if (options.value.PageNo == 0) {
      loadCountLaw();
    }
    axios
		.post(
			baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_documents_list",
						par: [
							{ par: "organization_id", va: store.getters.user.organization_id },
							{ par: "user_id ", va: store.getters.user.user_id },
							{ par: "pageno", va: options.value.PageNo },
							{ par: "pagesize", va: options.value.PageSize },
							{ par: "search", va: options.value.SearchText },
							{ par: "law_type", va: options.value.LawType },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data)[0];
			if (isFirst.value) isFirst.value = false;
			let listLawRelate = [];
			data.forEach((element, i) => {
				element.is_order = options.value.PageNo * options.value.PageSize + i + 1;			
				if (element.publish_date != null) {
					var date1 = element.publish_date.split(" ");
					element.publish_date = date1[0];
				}
				if (element.issued_date != null) {
					var date1 = element.issued_date.split(" ");
					element.issued_date = date1[0];
				}
				if (element.expiration_date != null) {
					var date1 = element.expiration_date.split(" ");
					element.expiration_date = date1[0];
				}
				if (element.modified_date != null) {
					var date1 = element.modified_date.split(" ");
					element.modified_date = date1[0];
				}
			});
			datalists.value = data;
			countListLaw.value = datalists.value.length;
			if (rf == 'first') {
				loadAllDocsLaw();
				if (datalists.value.length > 0) {
					showDetailLaw(datalists.value[0]);
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
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
				confirmButtonText: "OK",
			});
			store.commit("gologout");
			}
		});
	}
};
let files = [];
const onUploadFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
const removeFile = (event) => {
  files = [];
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
//Thêm bản ghi
const saveLaw = (isFormValid) => {
	submitted.value = true;
	if (!isFormValid) {
		return;
	}
	if (law.value.law_name != null) {
		law.value.law_name = law.value.law_name.trim();
	}
	if(law.value.law_name.length >= 1500){
		swal.fire({
			title: "Thông báo!",
			text: "Tên văn bản luật không được vượt quá 1500 kí tự!",
			icon: "error",
			confirmButtonText: "OK",
		});
		return false;
	}
	if(law.value.law_number.length >= 250){
		swal.fire({
			title: "Thông báo!",
			text: "Số hiệu văn bản luật không được vượt quá 250 kí tự!",
			icon: "error",
			confirmButtonText: "OK",
		});
		return false;
	}
	if (typeof law.value.publish_date == "string") {
		var startDay = law.value.publish_date.split("/");
		law.value.publish_date = new Date(
		startDay[2] + "/" + startDay[1] + "/" + startDay[0]
		);
	}
	if (typeof law.value.issued_date == "string") {
		var endDay = law.value.issued_date.split("/");
		law.value.issued_date = new Date(
		endDay[2] + "/" + endDay[1] + "/" + endDay[0]
		);
	}
	if (typeof law.value.expiration_date == "string") {
		var endDay = law.value.expiration_date.split("/");
		law.value.expiration_date = new Date(
		endDay[2] + "/" + endDay[1] + "/" + endDay[0]
		);
	}
	let formData = new FormData();
	if (files.length == 0 && listFileUploaded.value.length == 0) {
		swal.fire({
			title: "Thông báo!",
			text: "Chọn file văn bản luật trước khi lưu!",
			icon: "error",
			confirmButtonText: "OK",
		});
		return false;
	}
	for (var i = 0; i < files.length; i++) {
		let file = files[i];
		formData.append("url_file", file);
	}

	if (law.value.key_tags != null) {
		law.value.tags = law.value.key_tags.toString();
	}
	if (law.value.key_fields != null && law.value.key_fields.length > 0) {
		law.value.field_name = "";
		law.value.key_fields.forEach((element, i) => {
			if (law.value.field_name != "") {
				law.value.field_name += ', ';
			}
			law.value.field_name += element.name;
		});
	}
	else {
		law.value.field_name = null;
	}
	
	let listLawRelate = [];
	if (law.value.relate_law != null && law.value.relate_law != "") {
		if (law.value.relate_law.length > 0) {
			law.value.relate_law.forEach((element, i) => {
				let relateLaw = { law_id: element.law_id, law_name: element.law_name };
				listLawRelate.push(relateLaw);
			});
		}
	}
	
	let listLawReplace = [];
	if (law.value.replace_law != null && law.value.replace_law != "") {
		if (law.value.replace_law.length > 0) {
			law.value.replace_law.forEach((element, i) => {
				let replaceLaw = { law_id: element.law_id, law_name: element.law_name };
				listLawReplace.push(replaceLaw);
			});
		}
	}
	if (law.value.summary == null) {
		law.value.summary = "";
	}
	formData.append("law_docs", JSON.stringify(law.value));
	formData.append("law_relates", JSON.stringify(listLawRelate));
	formData.append("law_replaces", JSON.stringify(listLawReplace));
	if (!isAddLaw.value) {
		listFileDel.value = listFileDel.value.filter(x => listFileUploaded.value.filter(y => y.file_id == x.file_id).length == 0);
		formData.append("fileUploadOld", JSON.stringify(listFileDel.value));
	}
	
	swal.fire({
		width: 110,
		didOpen: () => {
		swal.showLoading();
		},
	});
	
	//if (!isAddLaw.value) {
		axios({
			method: isAddLaw.value ? 'post' : 'put',
			url: baseUrlCheck +
				`/api/law_documents/${isAddLaw.value ? "Add_Law" : "Update_Law"}`,
			data: formData,
			headers: {
				Authorization: `Bearer ${store.getters.token}`,
			},
		})
		.then((response) => {
			if (response.data.err == "2") {
				swal.fire({
					title: "Thông báo",
					text: response.data.ms,
					icon: "warning",
					confirmButtonText: "OK",
				});
			}
			else if (response.data.err != "1") {
				swal.close();
				toast.success("Cập nhật văn bản luật thành công!");
				if (isAddLaw.value) {
					closeDialog('reload');
				}
				else {
					closeDialog();
					loadDataLaw(true);
					showDetailLaw(detailLaw.value);
				}
			} else {
				//console.log(response.data.ms);
				swal.fire({
					title: "Thông báo",
					text: "Xảy ra lỗi khi cập nhật văn bản luật.",
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
	//}
	
};

const onSearch = (event) => {
  options.value.loading = true;
  isDynamicSQL.value = true;
  loadDataLaw(true);
};

const showFilter = ref(false);
const filterButs = ref();
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
  if (showFilter.value) {
    showFilter.value = false;
  } else {
    showFilter.value = true;
  }
};

const itemfilterButs = ref([
  { label: "Loại văn bản", typeFilter: 0 },
  { label: "Cơ quan ban hành", typeFilter: 1 },
  { label: "Lĩnh vực", typeFilter: 2 },
  { label: "Người ký", typeFilter: 3 },
  { label: "Tags", typeFilter: 4 },
  { label: "Ngày ban hành", typeFilter: 5 },
  { label: "Ngày hết hiệu lực", typeFilter: 6 },
]);
const filterLawType = ref();
const filterLawIssuePlace = ref();
const filterLawField = ref();
const filterLawSigner = ref();
const filterLawTags = ref();
const filterLawIssueDateFrom = ref();
const filterLawIssueDateTo = ref();
const filterLawExpDateFrom = ref();
const filterLawExpDateTo = ref();
const styleObj = ref();
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  border: "1px solid #2196f3 !important",
});
const reFilterLaw = () => {
  filterLawType.value = null;
  filterLawIssuePlace.value = null;
  filterLawField.value = null;
  filterLawSigner.value = null;
  filterLawTags.value = null;
  filterLawIssueDateFrom.value = null;
  filterLawIssueDateTo.value = null;
  filterLawExpDateFrom.value = null;
  filterLawExpDateTo.value = null;
  filterLaws();
  showFilter.value = false;
  styleObj.value = "";
};
const filterSQL = ref([]);
const filterLaws = () => {
	styleObj.value = "";
	filterSQL.value = [];
	let arr = [];
	let obj = {};
	if (filterLawType.value != null && filterLawType.value.length > 0) {
		let listStrType = "";
		filterLawType.value.forEach((el, i) => {
			listStrType += el.name + ",";
		});
		obj.key = "law_type";
		obj.filteroperator = "and";
		arr.push({
			//matchMode: "contains",
			//value: listStrType,
			matchMode: "containsMany",
			value: listStrType.substring(0, listStrType.length - 1),
		});
		obj.filterconstraints = arr;
		filterSQL.value.push(obj);
		styleObj.value = style.value;
	}
	let obj1 = {};
	if (filterLawIssuePlace.value != null && filterLawIssuePlace.value.length > 0) {
		let listStrIssuePlace = "";
		filterLawIssuePlace.value.forEach((el, i) => {
			listStrIssuePlace += el.name + ",";
		});
		obj1.key = "issue_place";
		obj1.filteroperator = "and";
		arr = [];
		arr.push({
			//matchMode: "contains",
			//value: listStrIssuePlace,
			matchMode: "containsMany",
			value: listStrIssuePlace.substring(0, listStrIssuePlace.length - 1),
		});
		obj1.filterconstraints = arr;
		filterSQL.value.push(obj1);
		styleObj.value = style.value;
	}
	let obj2 = {};
	if (filterLawField.value != null && filterLawField.value.length > 0) {
		let listStrField = "";
		filterLawField.value.forEach((el, i) => {
			listStrField += el.name + ",";
		});
		obj2.key = "field_name";
		obj2.filteroperator = "and";
		arr = [];
		arr.push({
			matchMode: "containsMany",
			value: listStrField.substring(0, listStrField.length - 1),
		});
		obj2.filterconstraints = arr;
		filterSQL.value.push(obj2);
		styleObj.value = style.value;
	}
	let obj3 = {};
	if (filterLawSigner.value != null && filterLawSigner.value.length > 0) {
		let listStrSigner = "";
		filterLawSigner.value.forEach((el, i) => {
			listStrSigner += el.name + ",";
		});
		obj3.key = "user_signed";
		obj3.filteroperator = "and";
		arr = [];
		arr.push({
			//matchMode: "contains",
			//value: listStrSigner,
			matchMode: "containsMany",
			value: listStrSigner.substring(0, listStrSigner.length - 1),
		});
		obj3.filterconstraints = arr;
		filterSQL.value.push(obj3);
		styleObj.value = style.value;
	}
	let obj4 = {};
	if (filterLawTags.value != null && filterLawTags.value.length > 0) {
		let listStrTag = "";
		filterLawTags.value.forEach((el, i) => {
			listStrTag += el + ",";
		});
		obj4.key = "tags";
		obj4.filteroperator = "and";
		arr = [];
		arr.push({
			matchMode: "containsMany",
			//value: filterLawTags.value.toString(),
			value: listStrTag.substring(0, listStrTag.length - 1),
		});
		obj4.filterconstraints = arr;
		filterSQL.value.push(obj4);
		styleObj.value = style.value;
	}
	let obj5 = {};
	if (filterLawIssueDateFrom.value != null) {
		if (typeof filterLawIssueDateFrom.value == "string") {
			var eDay = filterLawIssueDateFrom.value.split("/");
			filterLawIssueDateFrom.value = new Date(
				eDay[2] + "/" + eDay[1] + "/" + eDay[0]
			);
		}
		obj5.key = "issued_date";
		obj5.filteroperator = "and";
		arr = [];
		arr.push({
			matchMode: "dateAfter",
			value: filterLawIssueDateFrom.value,
		});
		obj5.filterconstraints = arr;
		filterSQL.value.push(obj5);
		styleObj.value = style.value;
	}
	let obj6 = {};
	if (filterLawIssueDateTo.value != null) {
		if (typeof filterLawIssueDateTo.value == "string") {
			var eDay = filterLawIssueDateTo.value.split("/");
			filterLawIssueDateTo.value = new Date(
				eDay[2] + "/" + eDay[1] + "/" + eDay[0]
			);
		}
		obj6.key = "issued_date";
		obj6.filteroperator = "and";
		arr = [];
		arr.push({
			matchMode: "dateBefore",
			value: filterLawIssueDateTo.value,
		});
		obj6.filterconstraints = arr;
		filterSQL.value.push(obj6);
		styleObj.value = style.value;
	}
	let obj7 = {};
	if (filterLawExpDateFrom.value != null) {
		if (typeof filterLawExpDateFrom.value == "string") {
			var eDay = filterLawExpDateFrom.value.split("/");
			filterLawExpDateFrom.value = new Date(
				eDay[2] + "/" + eDay[1] + "/" + eDay[0]
			);
		}
		obj7.key = "expiration_date";
		obj7.filteroperator = "and";
		arr = [];
		arr.push({
			matchMode: "dateAfter",
			value: filterLawIssueDateFrom.value,
		});
		obj7.filterconstraints = arr;
		filterSQL.value.push(obj7);
		styleObj.value = style.value;
	}	
	let obj8 = {};
	if (filterLawExpDateTo.value != null) {
		if (typeof filterLawExpDateTo.value == "string") {
			var eDay = filterLawExpDateTo.value.split("/");
			filterLawExpDateTo.value = new Date(
				eDay[2] + "/" + eDay[1] + "/" + eDay[0]
			);
		}
		obj8.key = "expiration_date";
		obj8.filteroperator = "and";
		arr = [];
		arr.push({
			matchMode: "dateBefore",
			value: filterLawIssueDateTo.value,
		});
		obj8.filterconstraints = arr;
		filterSQL.value.push(obj8);
		styleObj.value = style.value;
	}
	options.value.PageNo = 0;
	options.value.totalRecords = 0;
	options.value.id = null;
  	isDynamicSQL.value = true;
	loadDataSQL();
	showFilter.value = false;
};
const loadDataSQL = () => {
	let data = {
		id: options.value.id,
		next: options.value.IsNext,
		sqlF: store.getters.user.organization_id,
		sqlO: options.value.typeviews != 0 ? (typeViewsCount.value.filter(x => x.code == options.value.typeviews)[0].value + " DESC, ld.created_date") : "created_date",
		Search: options.value.SearchText,
		PageNo: options.value.PageNo,
		PageSize: options.value.PageSize,
		fieldSQLS: filterSQL.value,
	};
	
	options.value.loading = true;
	axios
		.post(baseUrlCheck +
			"/api/law_documents/Filter_Law_Documents", data, config)
		.then((response) => {
			if (response.data.err == "0") {
				let dt = JSON.parse(response.data.data);
				let data = dt[0];
				if (data.length > 0) {
					data.forEach((element, i) => {
						element.is_order = options.value.PageNo * options.value.PageSize + i + 1;
						if (element.publish_date != null) {
							var date1 = element.publish_date.split(" ");
							element.publish_date = date1[0];
						}
						if (element.issued_date != null) {
							var date1 = element.issued_date.split(" ");
							element.issued_date = date1[0];
						}
						if (element.expiration_date != null) {
							var date1 = element.expiration_date.split(" ");
							element.expiration_date = date1[0];
						}
						if (element.modified_date != null) {
							var date1 = element.modified_date.split(" ");
							element.modified_date = date1[0];
						}
					});
					datalists.value = data;
					countListLaw.value = datalists.value.length;
					showDetailLaw(data[0]);
				} else {
					datalists.value = null;
					countListLaw.value = 0;
					emitter.emit("emitData", {
						type: "reloadViewLaw",
						data:  null
					});
				}
				if (isFirst.value) isFirst.value = false;
				options.value.loading = false;
				//Show Count nếu có
				if (dt.length == 2) {
					options.value.totalRecords = dt[1][0].totalRecords;
				}
			}
			else {
				swal.fire({
					title: "Thông báo",
					text: response.data.err == "2" ? response.data.ms : "Lọc dữ liệu không thành công.",
					icon: "error",
					confirmButtonText: "OK",
				});
				return;
			}
		})
		.catch((error) => {
			options.value.loading = false;
			toast.error("Tải dữ liệu không thành công!");
			addLog({
				title: "Lỗi Console loadData",
				controller: "LawMain.vue",
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
const onRefresh = () => {
	showFilter.value = false;
	options.value = {
		IsNext: true,
		sort: "created_date",
		SearchText: "",
		PageNo: 0,
		PageSize: 20,
		loading: true,
		totalRecords: 0,
		LawType: "",
		typeviews: 0,
		organization_id: store.getters.user.organization_id,
		user_key: store.getters.user.user_key,
	};
	filterLawType.value = null;
	filterLawIssuePlace.value = null;
	filterLawField.value = null;
	filterLawSigner.value = null;
	filterLawTags.value = null;
	filterLawIssueDateFrom.value = null;
	filterLawIssueDateTo.value = null;
	filterLawExpDateFrom.value = null;
	filterLawExpDateTo.value = null;
	styleObj.value = "";
  	isDynamicSQL.value = false;
	loadDataLaw(true);
};

const downloadLaw = (data) => {
	updateNumView(data, 1);
	//window.open(baseUrlCheck + data.file_path, 'download');
	let a = document.createElement("a");
	//let url = baseUrlCheck + data.file_path;
	//a.href = url;
	let name = data.file_path.split("/").pop();
	a.href = basedomainURL + '/Viewer/DownloadFile?url='+ encodeURIComponent(data.file_path) + '&title=' + encodeURIComponent(name);
	//a.target = "_blank";
	a.download = name;
	document.body.appendChild(a);
	a.click();
	document.body.removeChild(a);
};
const updateNumView = (data, type) => {
	let formData = new FormData();
	formData.append("law_docs", JSON.stringify(data));
	formData.append("type_view", type);
	axios
		.put(baseUrlCheck +
			`/api/law_documents/Update_History_View`,
			formData,
			config,
		)
		.then((response) => {
		if (response.data.err != "1") {
			swal.close();
			listUserVisitor(data);
			//console.log("Update data number of view success.");
		} else {
			//console.log(response.data.ms);
			swal.fire({
			title: "Thông báo",
			text: "Xảy ra lỗi khi cập nhật lịch sử xem văn bản luật.",
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

const resetReplace = () => {
  law.value.replace_law = [];
};
const resetRelate = () => {
  law.value.relate_law = [];
};

const law_active = ref();
const detailLaw = ref({});
const listRelateLaw = ref([]);
const listReplaceLaw = ref([]);
const showDetailLaw = (lawShow) => {
	law_active.value = lawShow.law_id;
	axios
		.post(
			baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_documents_detail",
						par: [
							{ par: "law_id", va: lawShow.law_id },
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "organization_id", va: store.getters.user.organization_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			if (data.length > 0) {
				let reloadDataCmt = false;
				let ID_detailLaw_Pre = null;
				if (detailLaw.value && detailLaw.value.law_id != null) {
					reloadDataCmt = true;
					ID_detailLaw_Pre = detailLaw.value.law_id;
				}
				detailLaw.value = data[0][0];
				if (detailLaw.value.publish_date != null) {
					var date1 = detailLaw.value.publish_date.split(" ");
					detailLaw.value.publish_date = date1[0];
				}
				if (detailLaw.value.issued_date != null) {
					var date1 = detailLaw.value.issued_date.split(" ");
					detailLaw.value.issued_date = date1[0];
				}
				if (detailLaw.value.expiration_date != null) {
					var date1 = detailLaw.value.expiration_date.split(" ");
					detailLaw.value.expiration_date = date1[0];
				}
				if (detailLaw.value.created_date != null && detailLaw.value.modified_date == null) {
					detailLaw.value.modified_date = detailLaw.value.created_date;
				}
				if (detailLaw.value.modified_date != null) {
					var date1 = detailLaw.value.modified_date.split(" ");
					detailLaw.value.modified_date = date1[0];
				}

				if (detailLaw.value.field_name != null && detailLaw.value.field_name.length > 0) {
					if (!Array.isArray(detailLaw.value.field_name)) {
						let listFields = detailLaw.value.field_name.split(",");
						detailLaw.value.key_fields = [];
						listFields.forEach((element, i) => {
							let field = { name: element.trim(), code: element.trim() };
							detailLaw.value.key_fields.push(field);
						});				
					}
				}
				if (data[1].length > 0) {
					data[1].forEach((element, i) => {
						element.is_order = i + 1;
					});
				}
				listRelateLaw.value = data[1];

				if (data[2].length > 0) {
					data[2].forEach((element, i) => {
						element.is_order = i + 1;
					});
				}
				listReplaceLaw.value = data[2];
				
				updateNumView(detailLaw.value, 0);
				//listUserVisitor(detailLaw.value);
				if (ID_detailLaw_Pre != detailLaw.value.law_id) {
					reloadDataCmt = false;
					activeFirstTab();
				}
				if (ID_detailLaw_Pre != null) {
					emitter.emit("emitData", {
						type: "reloadViewLaw",
						data:  null
					});
				}
				else {
					emitter.emit("emitData", {
						type: "reloadViewLaw",
						data:  null,
					});
				}
			}
		})
		.catch((error) => {
			//console.log(error);
			toast.error("Tải dữ liệu không thành công!");

			if (error && error.status === 401) {
				swal.fire({
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};

const listUserView = ref([]);
const listUserDownload = ref([]);
const listUserVisitor = (lawShow) => {
	law_active.value = lawShow.law_id;
	axios
		.post(
			baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_user_visitor_list",
						par: [
							{ par: "law_id", va: lawShow.law_id },
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "organization_id", va: store.getters.user.organization_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			if (data.length > 0) {
				data[0].forEach((element, i) => {
					element.index = i;
				});
				listUserView.value = data[0];
				data[1].forEach((element, i) => {
					element.index = i;
				});
				listUserDownload.value = data[1];
			}
		})
		.catch((error) => {
			//console.log(error);
			toast.error("Tải dữ liệu theo dõi không thành công!");
			if (error && error.status === 401) {
				swal.fire({
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};

//Xóa văn bản luật
const delLaw = (Law) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá văn bản luật này không!",
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
          .delete(baseUrlCheck + "/api/law_documents/Delete_Law", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Law != null ? [Law.law_id] : "1",
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              	swal.close();
				toast.success("Xoá văn bản luật thành công!");
				options.value.PageNo = 0;
              	loadDataLaw('first');
			} else {
				//console.log(response.data.ms);
				swal.fire({
					title: "Thông báo",
					text: "Xảy ra lỗi khi xóa văn bản luật",
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

const tabLawActive = ref(0);
const activeFirstTab = () => {
	tabLawActive.value = 0;
}

// Import excel
const itemExport = "/Portals/Mau Excel/Mẫu Excel import văn bản luật.xlsx";
const Imp = ref(false);
const ImportLaw = () => {
	Imp.value = true;
};
let filesImport = [];
const removeFileImport = (event) => {
	filesImport = [];
};
const selectFileImport = (event) => {
	event.files.forEach((element) => {
		filesImport.push(element);
	});
};
const Upload = () => {
	Imp.value = false;
	let formData = new FormData();
	for (var i = 0; i < filesImport.length; i++) {
		let file = filesImport[i];
		formData.append("url_file", file);
	}
	swal.fire({
		width: 110,
		didOpen: () => {
		swal.showLoading();
		},
	});
	axios
		.post(
			baseUrlCheck + "/api/law_documents/ImportLaw",
			formData,
			config
		)
		.then((response) => {
			if (response.data.err == "0") {
				toast.success("Nhập dữ liệu thành công");
				isDynamicSQL.value = false;
				swal.close();
				loadDataLaw('first');
			}
			else {
				swal.fire({
					title: "Error!",
					text: "Xảy ra lỗi khi import văn bản luật!",
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

// Call component LawContent
const countListLaw = ref(0);
const reloadDataLaw = () => {
  	//dataComments.value = null;
	axios
		.post(
			baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_documents_detail",
						par: [
							{ par: "law_id", va: detailLaw.value.law_id },
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "organization_id", va: store.getters.user.organization_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			if (data.length > 0) {
				let reloadDataCmt = false;
				let ID_detailLaw_Pre = null;
				if (detailLaw.value != null && detailLaw.value.law_id != null) {
					reloadDataCmt = true;
					ID_detailLaw_Pre = detailLaw.value.law_id;
				}
				detailLaw.value = data[0][0];
				if (detailLaw.value.publish_date != null) {
					var date1 = detailLaw.value.publish_date.split(" ");
					detailLaw.value.publish_date = date1[0];
				}
				if (detailLaw.value.issued_date != null) {
					var date1 = detailLaw.value.issued_date.split(" ");
					detailLaw.value.issued_date = date1[0];
				}
				if (detailLaw.value.expiration_date != null) {
					var date1 = detailLaw.value.expiration_date.split(" ");
					detailLaw.value.expiration_date = date1[0];
				}
				if (detailLaw.value.created_date != null && detailLaw.value.modified_date == null) {
					detailLaw.value.modified_date = detailLaw.value.created_date;
				}
				if (detailLaw.value.modified_date != null) {
					var date1 = detailLaw.value.modified_date.split(" ");
					detailLaw.value.modified_date = date1[0];
				}

				if (detailLaw.value.field_name != null && detailLaw.value.field_name.length > 0) {
					if (!Array.isArray(detailLaw.value.field_name)) {
						let listFields = detailLaw.value.field_name.split(",");
						detailLaw.value.key_fields = [];
						listFields.forEach((element, i) => {
							let field = { name: element.trim(), code: element.trim() };
							detailLaw.value.key_fields.push(field);
						});				
					}
				}
				if (data[1].length > 0) {
					data[1].forEach((element, i) => {
						element.is_order = i + 1;
					});
				}
				listRelateLaw.value = data[1];

				if (data[2].length > 0) {
					data[2].forEach((element, i) => {
						element.is_order = i + 1;
					});
				}
				listReplaceLaw.value = data[2];
				
				//updateNumView(detailLaw.value, 0);
				listUserVisitor(detailLaw.value);
				if (ID_detailLaw_Pre != detailLaw.value.law_id) {
					reloadDataCmt = false;
					activeFirstTab();
				}
				if (!reloadDataCmt) {
					//loadComment();
				}
				else {
					emitter.emit("emitData", {
						type: "renderComment",
						data:  null
					});
				}
			}		
		})
		.catch((error) => {
			//console.log(error);
			toast.error("Tải dữ liệu không thành công!");
			options.value.loading = false;
			console.log(error);
			if (error && error.status === 401) {
				swal.fire({
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
//

onMounted(() => {
	if (!checkURL(window.location.pathname, store.getters.listModule)) {
		//router.back();
	}
	loadFieldsList();
	loadTypesList();
	loadSignersList();
	loadIssuePlacesList();
	loadDataLaw('first');
	return {
		options,
		isFirst,
		onPage,
		loadFieldsList,
		loadTypesList,
		loadSignersList,
		loadIssuePlacesList,
		loadDataLaw,
		loadCountLaw,
		openLaw,
		layout
	};
});
</script>
<template>
	<!-- <div class="surface-100 bg-white law-wrap"> -->
	<div class="law-wrap">
		<div class="law-header" style="margin: 0; padding: 0.5rem 1rem;">
			<div class="w-full flex">
				<div class="w-3 col-12 p-0 flex">
					<div class="col-6 p-0 flex">
						<Dropdown
							v-model="options.typeviews"
							:options="typeViewsCount"
							optionLabel="name"
							optionValue="code"
							placeholder="Mới nhất"
							class="w-full mr-3"
							@change="filterLaws"
						>
							<template #value="slotProps">
								<div class="p-dropdown-car-value flex text-justify"
									v-if="slotProps.value != null"
								>
									<div class="pr-2 text-justify">
										<i class="pi pi-align-left"></i>
									</div>
									<div class="text-justify flex format-center">
										{{ 
											slotProps.value == 0 ? "Mới nhất" :
											slotProps.value == 1 ? "Xem nhiều" :
											slotProps.value == 2 ? "Tải nhiều" : "Tất cả"
										}}
									</div>
								</div>
								<span v-else>
									{{ slotProps.placeholder }}
								</span>
							</template>
						</Dropdown>
					</div>	
					<div class="col-6 p-0 flex"></div>
				</div>
				<div class="w-full text-center flex">
					<span class="p-input-icon-left mr-3 w-full flex search-filter">
                        <i class="pi pi-search" />
                        <InputText type="text" spellcheck="false" v-model="options.SearchText" placeholder="Tìm kiếm văn bản luật"
                            v-on:keyup.enter="onSearch"
							class="inputtext-filter"
						>
						</InputText>
						<Button class="p-button-outlined bg-white btntext-filter"
							type="button" icon="pi pi-filter" 
							@click="toggleFilter" 
							aria-haspopup="true" 
							aria-controls="overlay_panel" 
            				:style="[styleObj]"
						/>
						<OverlayPanel
							ref="filterButs"
							appendTo="body"
							id="overlay_panel"
							:showCloseIcon="false"
							style="width:35%;"
						>
							<div class="grid formgrid m-0">
								<div class="col-12 md:col-12 p-0">
									<div class="flex" style="flex-direction: column;">
										<div class="col-12 flex mb-3">
											<div class="col-3 text-left pt-2 p-0" style="text-align: left">
												Loại văn bản
											</div>
											<MultiSelect v-model="filterLawType" 
												:options="listTypes" 
												optionLabel="name" 
												placeholder="Loại văn bản" 
												:filter="true"
												display="chip" 
												class="col-9 p-0 text-left"
											/>
										</div>
										<div class="col-12 flex mb-3">
											<div class="col-3 text-left pt-2 p-0" style="text-align: left">
												Cơ quan ban hành
											</div>
											<MultiSelect v-model="filterLawIssuePlace" 
												:options="listIssuePlaces" 
												optionLabel="name" 
												placeholder="Cơ quan ban hành"
												:filter="true" 
												display="chip" 
												class="col-9 p-0 text-left"
											/>
										</div>
										<div class="col-12 flex mb-3">
											<div class="col-3 text-left pt-2 p-0" style="text-align: left">
												Lĩnh vực
											</div>
											<MultiSelect v-model="filterLawField" 
												:options="listFields" 
												optionLabel="name" 
												placeholder="Lĩnh vực" 
												:filter="true"
												display="chip" 
												class="col-9 p-0 text-left"
											/>
										</div>
										<div class="col-12 flex mb-3">
											<div class="col-3 text-left pt-2 p-0" style="text-align: left">
												Người ký
											</div>
											<MultiSelect v-model="filterLawSigner" 
												:options="listSigners" 
												optionLabel="name" 
												placeholder="Người ký" 
												:filter="true"
												display="chip" 
												class="col-9 p-0 text-left"
											/>
										</div>
										<div class="col-12 flex mb-3">
											<div class="col-3 text-left pt-2 p-0" style="text-align: left">
												Tags
											</div>
											<Chips class="col-9 p-0 text-left" placeholder="Ấn Enter sau mỗi từ khóa" v-model="filterLawTags" />
										</div>
										<div class="col-12 flex mb-3">
											<div class="col-3 text-left pt-2 p-0" style="text-align: left">
												Ngày ban hành
											</div>
											<Calendar id="iconIssueDateFrom" v-model="filterLawIssueDateFrom" placeholder="dd/mm/yyyy"
												:manualInput="false"
												:showIcon="true"
												class="col-4 p-0 text-left"
											/>
											<span class="col-1 flex" style="justify-content: center; align-items: center;"> đến </span>
											<Calendar id="iconLawIssueDateTo" v-model="filterLawIssueDateTo" placeholder="dd/mm/yyyy"
												:manualInput="false"
												:showIcon="true"
												class="col-4 p-0 text-left"
											/>
										</div>											
										<div class="col-12">
											<div class="col-12 p-0 flex mb-3">
												<div class="col-3 text-left pt-2 p-0" style="text-align: left">
													Ngày hết hiệu lực
												</div>
												<Calendar id="iconExpDateFrom" v-model="filterLawExpDateFrom" placeholder="dd/mm/yyyy"
													:manualInput="false"
													:showIcon="true"
													class="col-4 p-0 text-left"
												/>
												<span class="col-1 flex" style="justify-content: center; align-items: center;"> đến </span>
												<Calendar id="iconLawExpDateTo" v-model="filterLawExpDateTo" placeholder="dd/mm/yyyy"
													:manualInput="false"
													:showIcon="true"
													class="col-4 p-0 text-left"
												/>
											</div>
											<Toolbar class="border-none surface-0 outline-none pb-0 px-0">
												<template #start>
													<Button class="p-button-outlined" label="Xóa"
														@click="reFilterLaw"
													></Button>
												</template>
												<template #end>
													<Button @click="filterLaws" label="Lọc"></Button>
												</template>
											</Toolbar>
										</div>
									</div>
								</div>
							</div>
						</OverlayPanel>
                    </span>
				</div>
				<div class="w-4 col-12 p-0 text-right flex" style="justify-content:flex-end;">
					<Button label="Thêm mới" icon="pi pi-plus" class="mr-2" @click="openLaw" />
					<Button label="Import" icon="pi pi-file-excel" class="mr-2" @click="ImportLaw()" />
                    <Button class="p-button-outlined p-button-secondary bg-white" icon="pi pi-refresh" @click="onRefresh" />
					<Button label="Xoá" icon="pi pi-trash" class="ml-2 p-button-danger" @click="delLaw(detailLaw)" v-if="detailLaw && detailLaw.allowDel" />
				</div>
			</div>
		</div>
		<div class="law-body" style="height:calc(100vh - 7.25rem); margin: 0;">
			<Splitter class="w-full" style="height:calc(100vh - 7.25rem)">
				<SplitterPanel :size="40">
					<div class="d-lang-table">
						<div class="col-12 flex" style="border-bottom: 1px solid #dee2e6;background-color: #ffffff;color: #000000;padding: 0.9rem !important;">
							<span class="flex align-items-center font-bold list-law">Danh sách văn bản luật 
								<Badge class="ml-2" :value="options.totalRecords" severity="danger"></Badge>
							</span>
						</div>
						<DataView 
							class="w-full h-full ptable p-datatable-sm flex flex-column table-list-law"
							:lazy="true"
							:value="datalists" 
							:layout="layout" 
							:loading="options.loading"
							:rowHover="true"
							responsiveLayout="scroll"
							:scrollable="true"
							:paginator="true"
							:rows="options.PageSize"
							:totalRecords="options.totalRecords"
							@page="onPage($event)"
							paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
							:rowsPerPageOptions="[20, 30, 50, 100, 200]"
							currentPageReportTemplate=""
							style="height: calc(100vh - 10.75rem) !important;"
						>
							<template #list="slotProps">
								<div class="grid p-2 m-0 w-full dataListLaw" 
									:class="law_active == slotProps.data.law_id ? 'row-active-law' : ''"
									@click="showDetailLaw(slotProps.data)"
									style="background-color: #fff;">
									<div class="w-3rem pt-2 flex" style="justify-content:center;line-height:1.5;">
										<div class="flex" style="flex-direction:column;align-items: center;">
											<span class="font-bold">{{slotProps.data.is_order}}</span>
											<img src="../../assets/image/expired.png" style="width:35px;transform: rotate(45deg);" v-tooltip="'Hết hiệu lực'" v-if="slotProps.data.exp_law"/>
										</div>
									</div>
									<div class="flex pl-2 py-0" style="width:calc(100% - 3rem);flex-direction: column;">
										<div class="col-12 flex">
											<span class="font-bold law-name"
												:class="law_active == slotProps.data.law_id ? 'active-law' : ''"
											>
												{{slotProps.data.law_name}}
											</span>
										</div>
										<div class="col-12 flex">
											<div class="info-law col-5">
												<i class="pi pi-file mr-2"></i>
												<span class="mr-2">
													<span class="font-bold">Số hiệu: </span>
													{{ slotProps.data.law_number }}
												</span>												
											</div>
											<div class="info-law col-5">
												<i class="pi pi-calendar mr-2"></i>
												<span class="mr-2">
													<span class="font-bold">Ban hành: </span>
													{{ slotProps.data.issued_date ? moment(new Date(slotProps.data.issued_date)).format("DD/MM/YYYY") : '' }}
												</span>
											</div>
											<div class="info-law col-2" style="justify-content:center;">
												<div class="btn-download-law"
													@click="downloadLaw(slotProps.data)"
												>
													<i class="pi pi-paperclip mr-2"></i>
													<span class="font-bold">Tải về </span>
												</div>
											</div>
										</div>
									</div>
								</div>
							</template>
							<template #empty>
								<div
								class="align-items-center justify-content-center p-4 text-center"
								v-if="!isFirst"
								>
								<img src="../../assets/background/nodata.png" height="144" />
								<h3 class="m-1">Không có dữ liệu</h3>
								</div>
							</template>
						</DataView>
					</div>
				</SplitterPanel>
				<SplitterPanel :size="60" style="min-width:45%;">
					<div class="comp-law" v-if="detailLaw != null && detailLaw.law_id != null">
						<detailsDocLaws
							:detailLaw="detailLaw"
							:listRelateLaw="listRelateLaw"
							:listReplaceLaw="listReplaceLaw"
							:refreshData="reloadDataLaw"
							:datalist_count="countListLaw"
							:listUserView="listUserView"
							:listUserDownload="listUserDownload"
							:listTypes="listTypes"
							:listIssuePlaces="listIssuePlaces"
							:listFields="listFields"
							:listSigners="listSigners"
							:allRelateLaw="allRelateLaw"
							:tabLawActive="0"
							:typeView="0"
						/>
					</div>					
				</SplitterPanel>
			</Splitter>
		</div>
	</div>
	<Dialog
		:header="headerLaw"
		v-model:visible="displayLaw"
    	:maximizable="true"
		:autoZIndex="true"
		:modal="true"
		style="z-index: 1000"
		:style="{ width: '60vw' }"
	>
		<!-- <form @submit.prevent="saveLaw(!v$.$invalid)"> -->
		<form @submit.prevent="saveLaw(!(v$.law_name.required.$invalid || v$.law_number.required.$invalid))">
			<div class="grid formgrid m-2">
				<div class="col-12 md:col-12 flex">
					<div class="col-12 md:col-12 p-0 m-0 flex">
						<label class="col-2 text-left flex" style="align-items:center;">
							Tên văn bản luật 
							<span class="redsao pl-1"> (*)</span>
						</label>
						<Textarea
							v-model="law.law_name"
							spellcheck="false"
							class="col-10 ip36"
							autoResize
							autofocus
              				:class="{ 'p-invalid': v$.law_name.$invalid && submitted }"
							style="padding:0.5rem;"
						/>
					</div>
				</div>
				<small class="col-12 p-error mt-2"
					v-if="(v$.law_name.required.$invalid && submitted) || v$.law_name.required.$pending.$response"
				>
					<div class="col-12 md:col-12 flex">
						<label class="col-2 text-left"></label>
						<span class="col-10 p-0">
              				{{
								v$.law_name.required.$message
									.replace("Value", "Tên văn bản luật")
									.replace("is required", "không được để trống")
							}}
						</span>
					</div>
				</small>
				<small class="col-12 p-error mt-2"
					v-if="(v$.law_name.maxLength.$invalid && submitted) || v$.law_name.maxLength.$pending.$response"
				>
					<div class="col-12 md:col-12 flex">
						<label class="col-2 text-left"></label>
						<span class="col-10 p-0">
							{{
								v$.law_name.maxLength.$message.replace(
									"The maximum length allowed is",
									"Tên văn bản luật không được vượt quá"
								)
							}}
							ký tự
						</span>
					</div>
				</small>
				<div class="field col-12 md:col-12 p-0 flex"></div>
				<div class="col-12 md:col-12 flex">
					<div class="flex col-6 md:col-6 p-0 m-0">
						<label class="flex col-4 text-left align-items-center">
							Số hiệu <span class="redsao pl-1"> (*)</span>
						</label>
						<InputText
							v-model="law.law_number"
							class="col-7 ip36"
            				:class="{ 'p-invalid': v$.law_number.$invalid && submitted }"
						/>
					</div>
					<div class="flex col-6 md:col-6 p-0 m-0">
                        <label class="flex col-4 text-left align-items-center">Ngày ban hành</label>
						<Calendar class="col-8 p-0" id="date_issued" v-model="law.issued_date" placeholder="dd/mm/yyyy" :manualInput="false" :showIcon="true" />
					</div>
				</div>
				<small class="col-12 p-error mt-2"
					v-if="(v$.law_number.required.$invalid && submitted) || v$.law_number.required.$pending.$response"
				>
					<div class="col-12 md:col-12 flex">
						<label class="col-2 text-left"></label>
						<span class="col-10 p-0">
              				{{
								v$.law_number.required.$message
									.replace("Value", "Số văn bản luật")
									.replace("is required", "không được để trống")
							}}
						</span>
					</div>
				</small>
				<small class="col-12 p-error mt-2"
					v-if="(v$.law_number.maxLength.$invalid && submitted) || v$.law_number.maxLength.$pending.$response"
				>
					<div class="col-12 md:col-12 flex">
						<label class="col-2 text-left"></label>
						<span class="col-10 p-0">
							{{
								v$.law_number.maxLength.$message.replace(
									"The maximum length allowed is",
									"Số hiệu văn bản luật không được vượt quá"
								)
							}}
							ký tự
						</span>
					</div>
				</small>
				<div class="field col-12 md:col-12 p-0 flex"></div>
				<div class="col-12 md:col-12 flex">
					<div class="field col-6 md:col-6 p-0">
						<label class="col-4 text-left">Số công báo </label>
						<InputText
							v-model="law.publish_number"
							class="col-7 ip36"
						/>
					</div>
					<div class="field col-6 md:col-6 p-0">
                        <label class="col-4 text-left">Ngày đăng công báo</label>
						<Calendar class="col-8 p-0" id="date_publish" v-model="law.publish_date" placeholder="dd/mm/yyyy" :manualInput="false" :showIcon="true" />
					</div>
				</div>				
				<div class="col-12 md:col-12 flex">
					<div class="field col-4 md:col-6 p-0 flex">
						<label class="col-4 mb-0 text-left flex" style="align-items:center;">Lĩnh vực</label>
						<MultiSelect v-model="law.key_fields" 
							:options="listFields" 
							optionLabel="name" 
							placeholder="Chọn lĩnh vực" 
							:filter="true"
							display="chip" 
							class="col-7 p-0 text-left"
						/>
					</div>
					<div class="field col-6 md:col-6 p-0">
                        <label class="col-4 text-left">Ngày hết hiệu lực</label>
						<Calendar class="col-8 p-0" id="date_publish" v-model="law.expiration_date" placeholder="dd/mm/yyyy" :manualInput="false" :showIcon="true" />
					</div>
				</div>
				<div class="col-12 md:col-12 flex">
					<div class="field col-4 md:col-6 p-0 flex">
						<label class="col-4 mb-0 text-left flex" style="align-items:center;">Loại văn bản</label>
						<Dropdown  class="col-7 p-0"
							spellcheck="false"
							v-model="law.law_type"
							:options="listTypes"
							optionLabel="name"
							optionValue="code"
							placeholder="Chọn loại văn bản"
							:editable="false"
							:filter="true"
							:virtualScrollerOptions="{ itemSize: 20, }"
						/>
					</div>
					<div class="field col-6 md:col-6 p-0 flex">
						<label class="col-4 mb-0 text-left flex" style="align-items:center;">Cơ quan ban hành</label>
						<Dropdown class="col-8 p-0"
							spellcheck="false"
							v-model="law.issue_place"
							:options="listIssuePlaces"
							optionLabel="name"
							optionValue="code"
							placeholder="Chọn cơ quan ban hành"
							:editable="false"
							:filter="true"
							:virtualScrollerOptions="{ itemSize: 20, }"
						/>
					</div>
				</div>				
				<div class="col-12 md:col-12 flex">
					<div class="field col-4 md:col-6 p-0 flex">
						<label class="col-4 mb-0 text-left flex" style="align-items:center;">Người ký</label>
						<Dropdown  class="col-7 p-0"
							spellcheck="false"
							v-model="law.user_signed"
							:options="listSigners"
							optionLabel="name"
							optionValue="code"
							placeholder="Chọn người ký"
							:editable="false"
							:filter="true"
						/>
					</div>
					<div class="field col-6 md:col-6 p-0">
						<label class="col-4 text-left">Ngày cập nhật</label>
						<Calendar class="col-8 p-0" id="date_publish" v-model="law.modified_date" :manualInput="false" :showIcon="true" disabled />
					</div>
				</div>
				<div class="col-12 md:col-12">
					<div class="field col-12 md:col-12 p-0 flex">
						<label class="col-2 mt-3 text-left">File văn bản luật<span class="redsao pl-1"> (*)</span></label>
						<div class="col-10 p-0">
							<FileUpload
								chooseLabel="Chọn File"
								:showUploadButton="false"
								:showCancelButton="false"
								:multiple="false"							
								:maxFileSize="10000000000"
								@select="onUploadFile"
								@remove="removeFile"
							/>
						</div>
					</div>
					<div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
						<label class="col-2 mt-3 text-left"></label>
						<div class="col-10 p-0 item-file-law">
							<DataView 
								class="w-full h-full ptable p-datatable-sm flex flex-column"
								:lazy="true"
								:value="listFileUploaded" 
								layout="list"
								:rowHover="true"
								responsiveLayout="scroll"
								:scrollable="true"
								>
								<template #list="slotProps">
									<div class="w-full">
										<Toolbar class="w-full">
											<template #start>
												<div class="flex align-items-center">
													<img class="mr-2"
														:src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
														style="object-fit: contain;"
														width="40" height="40"
													/>
													<span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
												</div>
											</template>
											<template #end>
												<Button
													icon="pi pi-times"
													class="p-button-rounded p-button-danger"
													@click="deleteFileLaw(slotProps.data)"
												/>
											</template>
										</Toolbar>
									</div>
								</template>
							</DataView>
						</div>
					</div>
				</div>
				<div class="col-12 md:col-12 flex">
					<div class="field col-12 md:col-12 p-0 flex">
						<label class="col-2 mb-0 text-left flex" style="align-items:center;">Tóm tắt</label>
						<div class="col-10 p-0">
							<Textarea
								spellcheck="false"
								v-model="law.summary"
								class="col-12 ip36"
								autoResize
								autofocus
								style="padding:0.5rem;"
							/>
						</div>
					</div>
				</div>
				<div class="col-12 md:col-12 flex">
					<div class="field col-12 md:col-12 p-0 flex">
						<label class="col-2 mb-0 text-left flex" style="align-items:center;">Văn bản liên quan </label>
						<MultiSelect v-model="law.relate_law" 
							:options="allRelateLaw" 
							optionLabel="law_name" 
							placeholder="Văn bản liên quan" display="chip"
							:filter="true"
							class="multiselect-custom col-10 p-0"
						>
							<template #value="slotProps">
								<div class="p-multiselect-car-token" v-for="option of slotProps.value" :key="option.law_id">
									<div>{{option.law_name}}</div>
								</div>
								<template v-if="!slotProps.value || slotProps.value.length === 0">
									Văn bản liên quan
								</template>
							</template>
							<template #option="slotProps">
								<div class="country-item" style="overflow:hidden;white-space:normal;">
									<div>{{slotProps.option.law_name}}</div>
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
				<div class="col-12 md:col-12 flex">
					<div class="field col-12 md:col-12 p-0 flex">
						<label class="col-2 mb-0 text-left flex" style="align-items:center;">Văn bản thay thế </label>
						<MultiSelect v-model="law.replace_law"
							:options="allRelateLaw" optionLabel="law_name"
							placeholder="Văn bản thay thế" display="chip"
							:filter="true"
							class="multiselect-custom col-10 p-0"							
						>
							<template #value="slotProps">
								<div class="p-multiselect-car-token" v-for="option of slotProps.value" :key="option.law_id">
									<div>{{option.law_name}}</div>
								</div>
								<template v-if="!slotProps.value || slotProps.value.length === 0">
									Văn bản thay thế
								</template>
							</template>
							<template #option="slotProps">
								<div class="country-item" style="overflow:hidden;white-space:normal;">
									<div>{{slotProps.option.law_name}}</div>
								</div>
							</template>
							<template #footer>
								<div>
									<Toolbar>
									<template #end>
										<div>
											<Button icon="pi pi-trash" class="mr-2 p-button-danger" label="Xóa" @click="resetReplace">
											</Button>
										</div>
									</template>
									</Toolbar>
								</div>
							</template>
						</MultiSelect>
					</div>
				</div>
				<div class="col-12 md:col-12 flex">
					<div class="field col-12 md:col-12 p-0 flex">
						<label class="col-2 mb-0 text-left flex" style="align-items:center;">Tags</label>
						<Chips class="col-10 p-0" placeholder="Ấn Enter sau mỗi từ khóa" v-model="law.key_tags" />
					</div>
				</div>
				<div class="col-12 md:col-12 flex">
					<div class="field col-6 md:col-6 p-0">
						<label style="vertical-align: text-bottom" class="col-4 text-left">Trạng thái </label>
						<InputSwitch v-model="law.is_active" class="col-8" />
					</div>
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

			<Button label="Lưu" icon="pi pi-check" @click="saveLaw(!(v$.law_name.required.$invalid || v$.law_number.required.$invalid))" />
		</template>
	</Dialog>
	<Dialog header="Tải lên file import văn bản luật"
		v-model:visible="Imp"
		:style="{ width: '40vw' }"
		:closable="true"
	>
		<h3>
			<label>
				<a :href="basedomainURL + itemExport" download>Nhấn vào đây</a> để tải xuống tệp mẫu.
			</label>
		</h3>
		<form>
			<FileUpload
				accept=".xls,.xlsx, .zip"
				@remove="removeFileImport"
				@select="selectFileImport"
				:multiple="true"
				:show-upload-button="false"
				:fileLimit="2"
				choose-label="Chọn tệp"
				cancel-label="Hủy"
			>
				<template #empty>
					<p>Kéo và thả tệp vào đây để tải lên. (File .zip, .xls, .xlsx)</p>
				</template>
			</FileUpload>
		</form>
		<template #footer>
			<Button label="Lưu" icon="pi pi-check" @click="Upload" />
		</template>
		<!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
	</Dialog>
</template>
<style scoped>
.law-wrap {
  height: 100%;
}

.law-header {
  border-top: 1px solid #e9ecef;
  padding: 0.5rem 0;
  margin-bottom: 0;
}

.law-body {
  height: 100%;
}
.css-icon {
  /* font-size: 16px; */
  font-size: 14px;
  font-weight: bold;
}
.info-law {
  display: flex;
  /* flex-grow: 1;
	flex-basic: 0; */
  padding: 0;
  font-size: 13px;
}
.btn-download-law {
  cursor: pointer;
}
.btn-download-law:hover {
  color: #007ad8;
}
.dataListLaw:hover .law-name {
  /* color: #0078d4; */
  cursor: pointer;
}
.law-name {
  line-height: 1.5;
}
.active-law {
  color: #0078d4;
  text-align: justify;
}
.row-active-law {
  background-color: #f0f8ff !important;
}
.inputtext-filter {
  border-right: none;
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
}
.btntext-filter {
  border-left: none;
  border-top-left-radius: 0;
  border-bottom-left-radius: 0;
  border-color: #d1d6dc;
  color: #686868;
}
.search-filter:focus .p-button.p-button-outlined.btntext-filter:enabled,
.search-filter:hover .p-button.p-button-outlined.btntext-filter:enabled {
  border-left: none;
  color: #686868;
  border-color: #2196f3;
}
.search-filter:hover .p-button.p-button-outlined.btntext-filter:enabled:hover {
  color: #2196f3;
}
.filter-data-law {
  color: #2196f3 !important;
}
.search-filter:focus .p-inputtext.inputtext-filter:enabled,
.search-filter:hover .p-inputtext.inputtext-filter:enabled {
  border-color: #2196f3;
}
.inputtext-filter:enabled:focus {
  border-color: #dee2e6;
}
.box-comment {
  flex: 18;
  display: flex;
  flex-direction: column;
  background-color: #f9f9f9;
  border-radius: 0.5rem;
}
.btn-func-law .p-button:enabled:hover {
    background: transparent;
	color: #0d89ec !important;
}
.btn-func-law .p-button:enabled:hover {
    background: transparent;
	color: #0d89ec !important;
}
</style>

<style lang="scss" scoped>
	::v-deep(.p-panel) {
		.p-panel-header{
			background-color: #fff;
			border: none;
			padding: 0.5rem 1rem;
			align-items: flex-start;
		}
		.p-panel-content{
			border-left: none;
			border-right: none;
			border-bottom: none;
			padding: 0 0.5rem;
		}
		.p-panel-title {
			line-height: 1.5;
			margin-right: 2rem;
			text-align: justify;
			text-overflow: ellipsis;
			overflow: hidden;
			display: -webkit-box;
			-webkit-line-clamp: 3;
			-webkit-box-orient: vertical;
		}
	}
	::v-deep(.p-dropdown) {
		.p-dropdown-trigger {
			visibility: visible !important;
		}
	}
	::v-deep(.p-tabview) {
		.p-tabview-nav-content {
			margin: 0 1rem;
		}
		.p-tabview-nav {
			border-bottom: 2px solid #dee2e6;
		}
		.p-tabview-nav li {
			margin: 0 2px 0 0;
		}
		.p-tabview-nav li:nth-last-child(2) {
			margin: 0;
		}
		.p-tabview-nav li .p-tabview-nav-link {
			color: #54505c;
			border-radius: 0;
			justify-content: center;
		}
		.p-tabview-nav li.p-highlight .p-tabview-nav-link {
			border-color: #2196F3;
			color: #2196F3;
		}
		.p-tabview-panels {
			padding: 0.5rem 1rem;
		}
		.p-tabview-panels .p-tabview-panel:first-child {
			margin-right: -1rem;
		}
		.p-tabview-panels .accordion-custom.tab-law .p-accordion-content {
			border-left: none;
			border-right: none;
			border-bottom: none;
			padding: 0 0 1rem;
		}
		.p-tabview-panels .p-accordion .p-accordion-tab:first-child .p-accordion-header .p-accordion-header-link,
		.p-tabview-panels .p-accordion .p-accordion-tab:first-child .p-accordion-header:not(.p-highlight):not(.p-disabled):hover .p-accordion-header-link {
			border-bottom: none; 
		}
		.p-tabview-panels .p-accordion .p-accordion-tab:not(:first-child) .p-accordion-header .p-accordion-header-link,
		.p-tabview-panels .p-accordion .p-accordion-tab:not(:first-child) .p-accordion-header:not(.p-highlight):not(.p-disabled):hover .p-accordion-header-link {
			border-top: 1px solid #dee2e6; 
		}
		.p-tabview-panels .p-accordion .p-dataview.p-dataview-list .p-dataview-content > .p-grid > div:first-child {
			border-top-width: 1px;
		}	
		.p-panel-header	.title-law span {
			line-height: 1.5;
			text-align: justify;
			text-overflow: ellipsis;
			overflow: hidden;
			display: -webkit-box;
			-webkit-line-clamp: 3;
			-webkit-box-orient: vertical;
		}
	}
	::v-deep(.p-dataview.table-list-law) {
		.p-dataview-content {
			margin-right: -0.25rem;
		}
		.p-dataview-content:hover::-webkit-scrollbar-thumb {
			background-color: #e0e0e0 !important ;
		}
		.p-dataview-content .p-grid {
			display: block;
		}
	}
	::v-deep(.p-multiselect-panel) {
		.p-multiselect-items .p-multiselect-item span {
			text-overflow: ellipsis;
			overflow: hidden;
		}
	}
	::v-deep(.item-file-law) {
		.p-toolbar-group-left {
			flex: 25;
			margin-right: 1rem;
		}
		.p-toolbar-group-right {
			flex: 1;
		}
	}
	::v-deep(.list-law) {
		.p-badge {
			min-width: 1.5rem;
			height: 1.5rem;
		}
	}
	::v-deep(.comp-law) {
		.ql-bubble .ql-editor {
		padding-left: 0.5rem;
		padding-right: 0.5rem;
		}
		.ql-bubble .ql-editor.ql-blank:focus::before {
			content: '';
		}
	}
</style>