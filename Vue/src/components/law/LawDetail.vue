<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import commentLaws from "../news/comment.vue";
import detailsDocLaws from "../law/LawDetail.vue";
import { change_unsigned, encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const toast = useToast();
const router = inject("router");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const isCheckLaw = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
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

emitter.on("emitData", (obj) => {
	switch (obj.type) {
		case "reloadViewLaw":
			isCheckLaw.value = false;
			reloadDataLaw();
			break;
		case "reloadDataCmt":
			reloadComment(obj.data);
			break;
		default:
			break;
	}
});
const props = defineProps({
	detailLaw: Object,
	listRelateLaw: Object,
	listReplaceLaw: Object,
	refreshData: Function,
	datalist_count: Intl,
	listUserView: Object,
	listUserDownload: Object,
	listTypes: Object,
	listIssuePlaces: Object,
	listFields: Object,
	listSigners: Object,
	allRelateLaw: Object,
	tabLawActive: Intl,
	typeView: Intl
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
const law = ref({
	law_name: "",
	law_number: "",
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
const headerLaw = ref();
const displayLaw = ref(false);

const allRelateLaw = ref(props.allRelateLaw);
const listFileUploaded = ref([]); // danh sách file đã up của văn bản luật
const listFileDel = ref([]); // danh sách file bị xóa khi cập nhật của văn bản luật
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
const deleteFileLaw = (value) => {
	listFileUploaded.value = listFileUploaded.value.filter(x => x.file_id != value.file_id);
};
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
			law_number: "",
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
		law_number: "",
		summary: "",
		tags: "",
		is_order: 1,
		is_active: true,
		is_new: 1,
		publish_date: null,
		issued_date: null,
		expiration_date: null,
	};
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
	if (files.length == 0 && listFileUploaded.length == 0) {
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
			displayLaw.value = false;
			emitter.emit("emitData", {
				type: "loadListLaw",
				data:  isAddLaw.value
			});
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
};

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

const showInfoLawDoc = ref(true);
const activeInfoLawDoc = () => {
	if (showInfoLawDoc.value) {
		showInfoLawDoc.value = false;
	} else {
		showInfoLawDoc.value = true;
	}
};

const displayModalIframeDoc = ref(false);
const openModalIframeDoc = ()=>{
    displayModalIframeDoc.value = true;
}
const searchUserView = ref('');
const filterListUserView = () => {	
	let filterU = change_unsigned(searchUserView.value);
	if (!filterU.length) return props.listUserView;
	return props.listUserView.filter(user => change_unsigned(user.full_name).includes(filterU) || user.user_id.includes(filterU));
}
const searchUserDown = ref('');
const filterListUserDown = () => {	
	let filterU = change_unsigned(searchUserDown.value);
	if (!filterU.length) return props.listUserDownload;
	return props.listUserDownload.filter(user => change_unsigned(user.full_name).includes(filterU) || user.user_id.includes(filterU));
}
const countListLaw = ref(props.datalist_count);
const tabLawActive = ref(props.tabLawActive);
const activeFirstTab = () => {
	tabLawActive.value = 0;
}
const changeTabContent = (event) => {
	tabLawActive.value = event.index;
}
const listRelateLaw = ref([]);
const listReplaceLaw = ref([]);
const detailLawComp = ref({});
const showViewLaw = (lawShow, type) => {
	isCheckLaw.value = true;
	axios
		.post(
			baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_documents_detail",
						par: [
							{ par: "law_id", va: type == 'relateView' ? lawShow.law_related_id : type == 'replaceView' ? lawShow.law_replace_id : lawShow.law_id },
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
				if (detailLawComp.value != null && detailLawComp.value.law_id != null) {
					reloadDataCmt = true;
					ID_detailLaw_Pre = detailLawComp.value.law_id;
				}
				detailLawComp.value = data[0][0];
				if (detailLawComp.value.publish_date != null) {
					var date1 = detailLawComp.value.publish_date.split(" ");
					detailLawComp.value.publish_date = date1[0];
				}
				if (detailLawComp.value.issued_date != null) {
					var date1 = detailLawComp.value.issued_date.split(" ");
					detailLawComp.value.issued_date = date1[0];
				}
				if (detailLawComp.value.expiration_date != null) {
					var date1 = detailLawComp.value.expiration_date.split(" ");
					detailLawComp.value.expiration_date = date1[0];
				}
				if (detailLawComp.value.created_date != null && detailLawComp.value.modified_date == null) {
					detailLawComp.value.modified_date = detailLawComp.value.created_date;
				}
				if (detailLawComp.value.modified_date != null) {
					var date1 = detailLawComp.value.modified_date.split(" ");
					detailLawComp.value.modified_date = date1[0];
				}

				if (detailLawComp.value.field_name != null && detailLawComp.value.field_name.length > 0) {
					if (!Array.isArray(detailLawComp.value.field_name)) {
						let listFields = detailLawComp.value.field_name.split(",");
						detailLawComp.value.key_fields = [];
						listFields.forEach((element, i) => {
							let field = { name: element.trim(), code: element.trim() };
							detailLawComp.value.key_fields.push(field);
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
				
				updateNumView(detailLawComp.value, 0);
				//listUserVisitor(detailLawComp.value);
				if (ID_detailLaw_Pre != detailLawComp.value.law_id) {
					reloadDataCmt = false;
					activeFirstTab();
				}
				
				emitter.emit("emitData", {
					type: "reloadDataCmt",
					data:  detailLawComp.value.law_id
				});
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
const reloadDataLaw = () => {
	dataComments.value = null;
	axios
		.post(
			baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_documents_detail",
						par: [
							{ par: "law_id", va: props.detailLaw.law_id },
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
				if (detailLawComp.value != null && detailLawComp.value.law_id != null) {
					reloadDataCmt = true;
					ID_detailLaw_Pre = detailLawComp.value.law_id;
				}
				detailLawComp.value = data[0][0];
				if (detailLawComp.value != null) {
					if (detailLawComp.value.publish_date != null) {
						var date1 = detailLawComp.value.publish_date.split(" ");
						detailLawComp.value.publish_date = date1[0];
					}
					if (detailLawComp.value.issued_date != null) {
						var date1 = detailLawComp.value.issued_date.split(" ");
						detailLawComp.value.issued_date = date1[0];
					}
					if (detailLawComp.value.expiration_date != null) {
						var date1 = detailLawComp.value.expiration_date.split(" ");
						detailLawComp.value.expiration_date = date1[0];
					}
					if (detailLawComp.value.created_date != null && detailLawComp.value.modified_date == null) {
						detailLawComp.value.modified_date = detailLawComp.value.created_date;
					}
					if (detailLawComp.value.modified_date != null) {
						var date1 = detailLawComp.value.modified_date.split(" ");
						detailLawComp.value.modified_date = date1[0];
					}

					if (detailLawComp.value.field_name != null && detailLawComp.value.field_name.length > 0) {
						if (!Array.isArray(detailLawComp.value.field_name)) {
							let listFields = detailLawComp.value.field_name.split(",");
							detailLawComp.value.key_fields = [];
							listFields.forEach((element, i) => {
								let field = { name: element.trim(), code: element.trim() };
								detailLawComp.value.key_fields.push(field);
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
					
					//updateNumView(detailLawComp.value, 0);
					listUserVisitor(detailLawComp.value);
					if (ID_detailLaw_Pre != detailLawComp.value.law_id) {
						reloadDataCmt = false;
						activeFirstTab();
					}
					reloadComment();
				}
			}
		})
		.catch((error) => {
			toast.error("Tải dữ liệu không thành công!");
			//console.log(error);
			if (error && error.status === 401) {
				swal.fire({
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
				confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
watch(props, () => {
	tabLawActive.value = props.isCheckTask;
});
// call component comment
const countComment = ref(0);
const lawComment = ref();
const dataCommentCheckList = ref();
const dataComments = ref([]);
const loadComment = (id) => {
	dataComments.value = null;
	lawComment.value = {
		des: null,
		law_id: props.detailLaw.law_id,
		user_id: store.getters.user.user_id,
	};
  	axios
    .post(
		baseUrlCheck + "api/law_documents/GetDataProc",
		{ 
			str: encr(JSON.stringify({
					proc: "law_comments_list",
					par: [
						{ par: "law_id", va: id || props.detailLaw.law_id },
						{ par: "user_id", va: store.getters.user.user_id },
					],
				}), SecretKey, cryoptojs
			).toString()
		},
		config
    )
    .then((response) => {
		let data = JSON.parse(response.data.data);
		data[0].forEach((element) => {
			if (element.created_date)
				element.created_date = new Date(moment(element.created_date).format("YYYY/MM/DD HH:mm:ss"));
			else
				element.created_date = new Date(moment(new Date()).format("YYYY/MM/DD HH:mm:ss"));
		});
		dataComments.value = data[0];
		countComment.value = data[1][0].c;
    })
    .catch((error) => {
		//console.log(error);
		toast.error("Tải dữ liệu không thành công!");
		//options.value.loading = false;
		if (error && error.status === 401) {
			swal.fire({
			text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
			confirmButtonText: "OK",
			});
			store.commit("gologout");
		}
    });
};
const optionsComment = ref({
	isShowInput: true,
	isUploadFile: true,
	isReply: false,
	isReaction: false,
});
const listCommentLaws = ref();
const reloadComment = (id) => {
  	dataComments.value = null;
	lawComment.value = {
		des: null,
		law_id: props.detailLaw.law_id,
		user_id: store.getters.user.user_id,
	};
	axios
		.post(
			baseUrlCheck + "api/law_documents/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_comments_list",
						par: [
							{ par: "law_id", va: id || props.detailLaw.law_id },
							{ par: "user_id", va: store.getters.user.user_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			data[0].forEach((element) => {
				if (element.created_date)
					element.created_date = new Date(moment(element.created_date).format("YYYY/MM/DD HH:mm:ss"));
				else
					element.created_date = new Date(moment(new Date()).format("YYYY/MM/DD HH:mm:ss"));
			});
			dataComments.value = data[0];
			countComment.value = data[1][0].c;
			emitter.emit("emitData", {
				type: "renderComment",
				data: null
			});
		})
		.catch((error) => {
			//console.log(error);
			toast.error("Tải dữ liệu không thành công!");
			//options.value.loading = false;
			if (error && error.status === 401) {
				swal.fire({
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
				confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};

const reloadViewLaw = () => {
  emitter.emit("emitData", { type: "reloadViewLaw", data: null });
};
onMounted(() => {
	loadComment();
	return {
		loadComment,
	}
});
</script>
<template>
	<div class="pt-0">
		<TabView lazy :active-index="tabLawActive" @tab-change="changeTabContent">
			<TabPanel>
				<template #header>
					<i class="pi pi-info-circle mr-2 css-icon"></i>
					<span>Nội dung</span>
				</template>
				<div class="law-viewer" v-if="props.datalist_count > 0">
					<div>
						<Panel :toggleable="false" :collapsed="false">
							<template #header>
								<div class="flex w-11 title-law" style="text-align: justify; font-weight: bold; line-height: 1.5;">
									<span>{{props.detailLaw ? props.detailLaw.law_name : 'Thông tin văn bản luật'}}</span>
								</div>
								<div class="flex w-1 btn-func-law" style="justify-content:right; flex-direction: column;">
									<div class="flex" style="justify-content:flex-end;">
										<Button class="mr-2 p-1" @click="openLaw(props.detailLaw)" v-tooltip.left="'Chỉnh sửa'" 
											style="background-color:transparent;border:none;color:#000;"
											v-if="props.detailLaw && props.detailLaw.allowDel && props.typeView == 0"
										>
											<i class="pi pi-pencil" style="font-size:1.2rem;"></i>
										</Button>
										<Button class="p-1" @click="openModalIframeDoc" v-tooltip.left="'Xem toàn màn hình'"
											style="background-color:transparent;border:none;color:#000;">
											<i class="pi pi-window-maximize" style="font-size:1.2rem;"></i>
										</Button>
										<Sidebar v-model:visible="displayModalIframeDoc" 
											position="full" style="z-index:1001;">
											<iframe style="height: calc(100vh - 3.3rem)" :src="basedomainURL + '/Viewer?title=' + encodeURIComponent(props.detailLaw.law_name) + '&url=' + encodeURIComponent(props.detailLaw.fileLawShow)" class="w-full"></iframe>
										</Sidebar>
									</div>
									<div class="flex" style="justify-content:flex-end;">
										<Button class="btn-collapse-law" @click="activeInfoLawDoc()" 
											style="background-color:transparent;border:none;color:#000;padding:0;"
											v-tooltip.left="showInfoLawDoc ? 'Thu gọn' : 'Chi tiết'"
										>
											<i :class="showInfoLawDoc ? 'pi pi-angle-down' : 'pi pi-angle-left'" style="font-size:1.5rem;"></i>
										</Button>
									</div>
								</div>
							</template>
							<div class="p-0 pb-2 mb-0 surface-0 w-full" v-if="props.detailLaw && showInfoLawDoc">
								<div class="col-12 p-0 flex">
									<div class="field flex col-6 md:col-6 p-0 mb-0">
										<label class="font-bold w-10rem m-0 py-1 px-2">Cơ quan ban hành: </label>
										<span class="py-1">{{ props.detailLaw.issue_place }}</span>
									</div>							
									<div class="field flex col-6 md:col-6 p-0 mb-0">
										<label class="font-bold w-10rem m-0 py-1 px-2">Số công báo: </label>
										<span class="py-1">{{ props.detailLaw.publish_number }}</span>
									</div>
								</div>
								<div class="col-12 p-0 flex">
									<div class="field flex col-6 md:col-6 p-0 mb-0">
										<label class="font-bold w-10rem m-0 py-1 px-2">Số hiệu: </label>
										<span class="py-1">{{ props.detailLaw.law_number }}</span>
									</div>
									<div class="field flex col-6 md:col-6 p-0 mb-0">
										<label class="font-bold w-10rem m-0 py-1 px-2">Ngày công báo: </label>
										<span class="py-1">{{ props.detailLaw.publish_date ? moment(new Date(props.detailLaw.publish_date)).format("DD/MM/YYYY") : '' }}</span>
									</div>
								</div>
								<div class="col-12 p-0 flex">
									<div class="field flex col-6 md:col-6 p-0 mb-0">
										<label class="font-bold w-10rem m-0 py-1 px-2">Loại văn bản: </label>
										<span class="py-1">{{ props.detailLaw.law_type }}</span>
									</div>
									<div class="field flex col-6 md:col-6 p-0 mb-0">
										<label class="font-bold w-10rem m-0 py-1 px-2">Người ký: </label>
										<span class="py-1">{{ props.detailLaw.user_signed }}</span>
									</div>
								</div>
								<div class="col-12 p-0 flex">
									<div class="field flex col-6 md:col-6 p-0 mb-0">
										<label class="font-bold w-10rem m-0 py-1 px-2">Ngày ban hành: </label>
										<span class="py-1">{{ props.detailLaw.issued_date ? moment(new Date(props.detailLaw.issued_date)).format("DD/MM/YYYY") : '' }}</span>
									</div>
									<div class="field flex col-6 md:col-6 p-0 mb-0">
										<label class="font-bold w-10rem m-0 py-1 px-2">Ngày hết hiệu lực: </label>
										<span class="py-1">{{ props.detailLaw.expiration_date ? moment(new Date(props.detailLaw.expiration_date)).format("DD/MM/YYYY") : '' }}</span>
									</div>
								</div>
								<div class="col-12 p-0 flex">
									<div class="field flex col-6 md:col-6 p-0 mb-0">
										<label class="font-bold w-10rem m-0 py-1 px-2">Lĩnh vực: </label>
										<span class="py-1">{{ props.detailLaw.field_name != null ? (props.detailLaw.field_name.replaceAll(',', ', ')) : props.detailLaw.field_name }}</span>
									</div>
									<div class="field flex col-6 md:col-6 p-0 mb-0">
										<label class="font-bold w-10rem m-0 py-1 px-2">Ngày cập nhật: </label>
										<span class="py-1">{{ props.detailLaw.modified_date ? moment(new Date(props.detailLaw.modified_date)).format("DD/MM/YYYY") : '' }}</span>
									</div>
								</div>
							</div>
						</Panel>
					</div>
					<iframe class="w-full" :src="(basedomainURL + '/Viewer?title=' + encodeURIComponent(props.detailLaw.law_name) + '&url=' + encodeURIComponent(props.detailLaw.fileLawShow))"
						style="border: 2px solid #d9d9d9;"
						:style="props.typeView == 0 ? (showInfoLawDoc ? 'min-height: calc(100vh - 24.5rem);' : 'min-height: calc(100vh - 15.5rem);')
								: (showInfoLawDoc ? 'min-height: calc(100vh - 19.5rem);' : 'min-height: calc(100vh - 10.5rem);')"	
					></iframe>
				</div>
				<div class="align-items-center justify-content-center p-4 text-center" v-else>
					<img src="../../assets/background/nodata.png" height="144" />
					<h3 class="m-1">Không có dữ liệu</h3>
				</div>
			</TabPanel>
			<TabPanel>
				<template #header>
					<i class="pi pi-file mr-2 css-icon"></i>
					<span>Văn bản liên quan</span>
				</template>
				<Accordion class="accordion-custom tab-law" :activeIndex="props.listRelateLaw.length > 0 ? 0 : props.listReplaceLaw.length > 0 ? 1 : null" v-if="props.datalist_count > 0">
					<AccordionTab>
						<template #header>
							<i class="pi pi-link pr-2 font-bold"></i>
							<span>Văn bản liên quan ({{props.listRelateLaw.length}})</span>
						</template>
						<DataView 
							class="w-full h-full ptable p-datatable-sm flex flex-column"
							:lazy="true"
							:value="props.listRelateLaw" 
							layout="list"
							:rowHover="true"
							responsiveLayout="scroll"
							:scrollable="true"
							style="max-height: calc(100vh - 270px);"
						>
							<template #list="slotProps">
								<div class="grid p-2 m-0 w-full dataListLaw"
									style="background-color: #fff;"
									@click="showViewLaw(slotProps.data, 'relateView')"
								>
									<div class="w-2rem pt-2 flex" style="justify-content:center;line-height:1.5;">
										<div class="flex">
											<span class="font-bold">{{slotProps.data.is_order}}</span>
										</div>
									</div>
									<div class="flex pl-2 py-0" style="width:calc(100% - 2rem);flex-direction: column;">
										<div class="col-12 flex">
											<span class="font-bold law-name">
												{{slotProps.data.law_relate_name}}
											</span>
										</div>														
									</div>
								</div>
							</template>
						</DataView>
					</AccordionTab>
					<AccordionTab>
						<template #header>
							<i class="pi pi-sync pr-2 font-bold"></i>
							<span>Văn bản thay thế ({{props.listReplaceLaw.length}})</span>
						</template>
						<DataView 
							class="w-full h-full ptable p-datatable-sm flex flex-column"
							:lazy="true"
							:value="props.listReplaceLaw" 
							layout="list"
							:rowHover="true"
							responsiveLayout="scroll"
							:scrollable="true"
							style="max-height: calc(100vh - 260px);"
						>
							<template #list="slotProps">
								<div class="grid p-2 m-0 w-full dataListLaw"
									style="background-color: #fff;"
									@click="showViewLaw(slotProps.data, 'replaceView')"
								>
									<div class="w-2rem pt-2 flex" style="justify-content:center;line-height:1.5;">
										<div class="flex">
											<span class="font-bold">{{slotProps.data.is_order}}</span>
										</div>
									</div>
									<div class="flex pl-2 py-0" style="width:calc(100% - 2rem);flex-direction: column;">
										<div class="col-12 flex">
											<span class="font-bold law-name">
												{{slotProps.data.law_replace_name}}
											</span>
										</div>														
									</div>
								</div>
							</template>
						</DataView>
					</AccordionTab>
				</Accordion>
				<div class="align-items-center justify-content-center p-4 text-center" v-else>
					<img src="../../assets/background/nodata.png" height="144" />
					<h3 class="m-1">Không có dữ liệu</h3>
				</div>
			</TabPanel>
			<TabPanel>
				<template #header>
					<i class="pi pi-comments mr-2 css-icon"></i>
					<span>Lưu ý</span>
				</template>
				<div v-if="props.datalist_count > 0 && dataComments != null">		
					<commentLaws
						:options="optionsComment"
						:refreshData="reloadComment"
						:objectData="lawComment"
						:comment_count="countComment"
						:dataComments="dataComments"
						:Controller="'law_comment'"
					/>
				</div>	
				<div class="align-items-center justify-content-center p-4 text-center" v-else>
					<img src="../../assets/background/nodata.png" height="144" />
					<h3 class="m-1">Không có dữ liệu</h3>
				</div>	
			</TabPanel>
			<TabPanel>
				<template #header>
					<i class="pi pi-history mr-2 css-icon"></i>
					<span>Theo dõi</span>
				</template>
				<Accordion class="accordion-custom tab-law" :activeIndex="props.listUserView.length > 0 ? 0 : props.listUserDownload.length > 0 ? 1 : null" v-if="props.datalist_count > 0">
					<AccordionTab>
						<template #header>
							<i class="pi pi-eye pr-2 font-bold"></i>
							<span>Xem văn bản luật ({{props.listUserView.length}})</span>
						</template>
						<DataView 
							class="w-full h-full ptable p-datatable-sm flex flex-column"
							:lazy="true"
							:value="filterListUserView()" 
							layout="list"
							:rowHover="true"
							responsiveLayout="scroll"
							:scrollable="true"
							style="max-height: calc(100vh - 270px);"
						>
							<template #header>
								<div class="grid w-full p-0 m-0" style="border-top: 1px solid #dee2e6;border-bottom: 1px solid #e9ecef;">
									<div class="col-8 flex" style="flex:1;align-items:center;">
										<h4 class="m-0">Danh sách người xem ({{(filterListUserView()).length}})</h4>
									</div>
									<div class="col-4 flex">
										<span class="p-input-icon-left w-full">
											<i class="pi pi-search" />
											<InputText
												type="text"
												class="p-inputtext-sm"
												spellcheck="false"
												placeholder="Tìm kiếm"
												v-model="searchUserView"
												style="font-size:1rem;"
											/>
										</span>
									</div>
								</div>
							</template>
							<template #list="slotProps">
								<div class="grid w-full p-0">
									<div
										class="field col-12 flex m-0 cursor-pointer"
										:style="
											slotProps.data.active
											? 'background-color:#bed3f5'
											: 'background-color:none'
										"
									>
										<div class="col-1 p-0 flex align-items-center justify-content-center">
											<Avatar
												v-bind:label="slotProps.data.avatar ? '' : slotProps.data.last_name.substring(0, 1)"
												v-bind:image="basedomainURL + slotProps.data.avatar"
												:style="{ background: bgColor[slotProps.data.index % 7], }"
												class="mt-2"
												size="large"
												shape="circle"
											/>
										</div>
										<div class="col-7 p-0 pl-2">
											<div class="pt-2" style="line-height: 1.5;">
												<div class="font-bold">
													{{ slotProps.data.full_name }}
													<span class="pl-1 font-normal">{{ "(" + slotProps.data.user_id + ")" }}</span>
												</div>
												<div class="flex w-full" style="flex-direction: column;">
													<div class="flex">Phòng ban: {{ slotProps.data.phongban || '' }}</div>
													<div class="flex">Chức vụ: {{ slotProps.data.chucvu || '' }}</div>
												</div>
											</div>
										</div>
										<div class="col-4 p-0 pl-2 flex align-items-center" style="line-height: 1.5;">
											<div class="flex w-full pt-2" style="flex-direction: column;">
												<div class="flex">
													<span class="font-bold pr-2">Số lượt xem: </span>
													<span class="font-bold" style="color:#2196F3;">{{ slotProps.data.times }}</span>
												</div>
												<div class="flex">
													<span class="pr-2">Xem lần đầu: </span>
													<span>{{ slotProps.data.created_date ? moment(new Date(slotProps.data.created_date)).format("HH:mm DD/MM/YYYY") : '' }}</span>
												</div>
												<div class="flex">
													<span class="pr-2">Xem gần nhất : </span>
													<span>{{ slotProps.data.modified_date ? moment(new Date(slotProps.data.modified_date)).format("HH:mm DD/MM/YYYY") : '' }}</span>
												</div>
											</div>
										</div>
									</div>
								</div>
							</template>
						</DataView>
					</AccordionTab>
					<AccordionTab>
						<template #header>
							<i class="pi pi-download pr-2 font-bold"></i>
							<span>Tải văn bản luật ({{props.listUserDownload.length}})</span>
						</template>
						<DataView 
							class="w-full h-full ptable p-datatable-sm flex flex-column"
							:lazy="true"
							:value="filterListUserDown()" 
							layout="list"
							:rowHover="true"
							responsiveLayout="scroll"
							:scrollable="true"
							style="max-height: calc(100vh - 260px);"
						>
							<template #header>
								<div class="grid w-full p-0 m-0" style="border-top: 1px solid #dee2e6;border-bottom: 1px solid #e9ecef;">
									<div class="col-8 flex" style="flex:1;align-items:center;">
										<h4 class="m-0">Danh sách người tải ({{(filterListUserDown()).length}})</h4>
									</div>
									<div class="col-4 flex">
										<span class="p-input-icon-left w-full">
											<i class="pi pi-search" />
											<InputText
												type="text"
												class="p-inputtext-sm"
												spellcheck="false"
												placeholder="Tìm kiếm"
												v-model="searchUserDown"
												style="font-size:1rem;"
											/>
										</span>
									</div>
								</div>
							</template>
							<template #list="slotProps">
								<div class="grid w-full p-0">
									<div
										class="field col-12 flex m-0 cursor-pointer"
										:style="
											slotProps.data.active
											? 'background-color:#bed3f5'
											: 'background-color:none'
										"
									>
										<div class="col-1 p-0 flex align-items-center justify-content-center">
											<Avatar
												v-bind:label="slotProps.data.avatar ? '' : slotProps.data.last_name.substring(0, 1)"
												v-bind:image="basedomainURL + slotProps.data.avatar"
												:style="{ background: bgColor[slotProps.data.index % 7], }"
												class="mt-2"
												size="large"
												shape="circle"
											/>
										</div>
										<div class="col-7 p-0 pl-2">
											<div class="pt-2" style="line-height: 1.5;">
												<div class="font-bold">
													{{ slotProps.data.full_name }}
													<span class="pl-1 font-normal">{{ "(" + slotProps.data.user_id + ")" }}</span>
												</div>
												<div class="flex w-full" style="flex-direction: column;">
													<div class="flex">Phòng ban: {{ slotProps.data.phongban || '' }}</div>
													<div class="flex">Chức vụ: {{ slotProps.data.chucvu || '' }}</div>
												</div>
											</div>
										</div>
										<div class="col-4 p-0 pl-2 flex align-items-center" style="line-height: 1.5;">
											<div class="flex w-full pt-2" style="flex-direction: column;">
												<div class="flex">
													<span class="font-bold pr-2">Số lượt tải: </span>
													<span class="font-bold" style="color:#2196F3;">{{ slotProps.data.times }}</span>
												</div>
												<div class="flex">
													<span class="pr-2">Tải lần đầu: </span>
													<span>{{ slotProps.data.created_date ? moment(new Date(slotProps.data.created_date)).format("HH:mm DD/MM/YYYY") : '' }}</span>
												</div>
												<div class="flex">
													<span class="pr-2">Tải gần nhất : </span>
													<span>{{ slotProps.data.modified_date ? moment(new Date(slotProps.data.modified_date)).format("HH:mm DD/MM/YYYY") : '' }}</span>
												</div>
											</div>
										</div>
									</div>
								</div>
							</template>
						</DataView>
					</AccordionTab>
				</Accordion>
				<div class="align-items-center justify-content-center p-4 text-center" v-else>
					<img src="../../assets/background/nodata.png" height="144" />
					<h3 class="m-1">Không có dữ liệu</h3>
				</div>
			</TabPanel>
		</TabView>
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
		<form @submit.prevent="saveLaw(!v$.$invalid)">
			<div class="grid formgrid m-2">
				<div class="field col-12 md:col-12 flex">
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
				<small class="col-12 p-error"
					v-if="(v$.law_name.required.$invalid && submitted) || v$.law_name.required.$pending.$response"
				>
					<div class="field col-12 md:col-12 flex">
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
				<small class="col-12 p-error"
					v-if="(v$.law_name.maxLength.$invalid && submitted) || v$.law_name.maxLength.$pending.$response"
				>
					<div class="field col-12 md:col-12 flex">
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
				<div class="field col-12 md:col-12 flex">
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
				<small class="col-12 p-error"
					v-if="(v$.law_number.required.$invalid && submitted) || v$.law_number.required.$pending.$response"
				>
					<div class="field col-12 md:col-12 flex">
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
				<small class="col-12 p-error"
					v-if="(v$.law_number.maxLength.$invalid && submitted) || v$.law_number.maxLength.$pending.$response"
				>
					<div class="field col-12 md:col-12 flex">
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
							:options="props.listFields" 
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
							:options="props.listTypes"
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
							:options="props.listIssuePlaces"
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
							:options="props.listSigners"
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
							:options="props.allRelateLaw" 
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
							:options="props.allRelateLaw" optionLabel="law_name"
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

			<Button label="Lưu" icon="pi pi-check" @click="saveLaw(!v$.$invalid)" />
		</template>
	</Dialog>
	<Sidebar
		v-model:visible="isCheckLaw"
		:showCloseIcon="false"
		position="right"
		class="p-sidebar-lg sidebar-law"
		style="width:55%;"
		v-on:hide="reloadViewLaw()"
	>
		<detailsDocLaws
			:detailLaw="detailLawComp"
			:listRelateLaw="listRelateLaw"
			:listReplaceLaw="listReplaceLaw"
			:refreshData="reloadDataLaw"
			:datalist_count="countListLaw"
			:listUserView="listUserView"
			:listUserDownload="listUserDownload"
			:listTypes="props.listTypes"
			:listIssuePlaces="props.listIssuePlaces"
			:listFields="props.listFields"
			:listSigners="props.listSigners"
			:allRelateLaw="props.allRelateLaw"
			:tabLawActive="0"
			:typeView="1"
		/>
	</Sidebar>
</template>

<style scoped>

</style>
<style lang="scss" scoped>	
	::v-deep(.p-tabview) {
		.p-tabview-nav-container {
			padding-left: 1rem;
			padding-right: 1rem;
		}
		.p-tabview-panels {
			padding: 0.5rem 1rem;
		}
	}
	::v-deep(.p-panel) {
		.p-panel-header {
			background-color: #fff;
			border: none;
			padding: 0.5rem 1rem;
			align-items: flex-start;
		}
		.p-panel-content {
			border-left: none;
			border-right: none;
			border-bottom: none;
			padding: 0 0.5rem;
		}
	}
</style>