<script setup>
import { defineProps, inject, ref, onMounted } from "vue";
import DocType from "./DocType.vue";
import DocStatus from "./DocStatus.vue";
import moment from "moment";
import DocLinkTask from "../../components/doc/DocLinkTask.vue";
import { integer } from "@vuelidate/validators";
const emitter = inject("emitter");
const store = inject("store");
const axios = inject("axios"); // inject axios
const swal = inject("$swal");
const basedomainURL = fileURL;
const baseUrlCheck = baseURL;

const props = defineProps({
  DocItem: Object,
  Type: String,
  key: Number,
  clickCheckbox: Function,
  resetCheckbox: Function
});
const passDocToDetail = (docitem) => {
	// updateViewDoc(docitem);
  props.resetCheckbox();
  emitter.emit("emitData", { type:"goToDetailDoc", data: docitem });
}
//--------- Checkbox ------------------
const checkDocBeforeChecked = (event) => {
  if(props.DocItem.checked){
    if(!props.DocItem.file_path){
      props.DocItem.checked = false;
      swal.fire({
          title: "Error!",
          text: "Văn bản không có file đính kèm!",
          icon: "error",
          confirmButtonText: "OK",
        });
    }
  }
  props.clickCheckbox(event);
}
//
// -------- Xem lien ket cong viec -------------------
const CloseDiaLogListLinkTask = () => {
  displayDialogListLinkTask.value = false;
}
const showModalListLinkTak = () => {
  headerDialogListLinkTask.value = "Công việc được liên kết";
  displayDialogListLinkTask.value = true;
}
const headerDialogListLinkTask = ref();
const displayDialogListLinkTask = ref(false);
//
function formatDate(date) {
  var d = new Date(date),
    month = '' + (d.getMonth() + 1),
    day = '' + d.getDate(),
    year = d.getFullYear();

  if (month.length < 2)
    month = '0' + month;
  if (day.length < 2)
    day = '0' + day;

  return [day, month, year].join('/');
}
const convertDateDBToString = (obj) => {
  for (var key in obj) {
    let isnum = /^\d+$/.test(obj[key]);
    if (!isnum && moment(obj[key], moment.ISO_8601, true).isValid()) {
      obj[key] = formatDate(obj[key]);
    }
  }
}
const classStyleDoc = ref({});
const refreshStyle = () => {
  classStyleDoc.value = {
    'doc-not-seen': (props.Type === 'receive' && (!props.DocItem.view_id || (!props.DocItem.view_date && props.DocItem.status_id !== props.DocItem.first_doc_status_id))),
    'doc-not-file': !props.DocItem.file_path && (props.DocItem.first_doc_status_id === 'sohoa' || props.DocItem.status_id === 'sohoa' || props.DocItem.status_id === 'dadongdau'),
    'count-deadline': props.Type === 'receive' && props.DocItem.date_deadline < 0 && (props.DocItem.status_id !== 'dadongdau' && props.DocItem.status_id !== 'phanphat' && props.DocItem.status_id !== 'hoanthanh')
  }
}
onMounted(() => {
  convertDateDBToString(props.DocItem);
  if(props.DocItem.isClicked) passDocToDetail(props.DocItem);
  refreshStyle();
  if(props.Type === 'receive'){
    props.DocItem.display_name = props.DocItem.send_by_name;
  }
  else if(props.Type === 'send'){
    props.DocItem.display_name = props.DocItem.receive_by_name;
  }
  else if(props.Type === 'store'){
    props.DocItem.display_name = props.DocItem.send_by_name;
  }
  if(props.DocItem.display_name){
    let arr_name = props.DocItem.display_name.split(" ");
    var last_name = arr_name[arr_name.length - 1];
    props.DocItem.avatar_char = last_name[0];
  }
  if(props.DocItem.urgency || props.DocItem.is_not_send_paper){
      props.DocItem.pre_info = '[';
      if(props.DocItem.urgency) props.DocItem.pre_info += props.DocItem.urgency;
      if(props.DocItem.is_not_send_paper){
        if(props.DocItem.urgency)
            props.DocItem.pre_info += ' - ';
        props.DocItem.pre_info += 'Không gửi bản giấy';
      } 
      props.DocItem.pre_info += ']';
  }
  emitter.on("emitData", (obj) => {
    switch (obj.type) {
        case "refreshDocAfterViewed":
          if(props.DocItem.isSelected){
            if(!props.DocItem.view_id) props.DocItem.view_id = 1;
            if(!props.DocItem.view_date && props.DocItem.follow_id) props.DocItem.view_date = new Date();
            refreshStyle();
          }
            break;
        default: break;
    }
});
  return {
    classStyleDoc
  };
});
</script>

<template>
    <div @click="passDocToDetail(DocItem)" class="col-12">
        <div @mouseover="DocItem.hover = true" @mouseleave="DocItem.hover = false" class="doc-list-item">
          <div class="doc-list-left">
            <div v-if="props.Type !== 'store'" class="doc-avatar">
                 <Avatar v-show="DocItem.avatar && !DocItem.hover && !DocItem.checked" class="avatar-image" :image="basedomainURL + (DocItem.avatar != null ? DocItem.avatar : '/Portals/Image/nouser1.png')" shape="circle" size="large"/>
                 <Avatar class="ava-text" v-show="!DocItem.avatar && !DocItem.hover && !DocItem.checked" :label="DocItem.avatar_char" shape="circle" size="large"/>
                 <Checkbox @change="checkDocBeforeChecked($event);" :binary="true" v-show="DocItem.hover || DocItem.checked" class="checkbox-radio" style="margin-right: 1.45rem" v-model="DocItem.checked" />
            </div>
          </div>
            <div class="doc-list-detail">
                <div v-if="props.Type !== 'store'" class="doc-name">{{ DocItem.display_name }}</div>
                <div class="doc-description" :class="classStyleDoc">{{ DocItem.pre_info }} {{ DocItem.compendium }}</div>
                <div class="doc-info">
                    Số ký hiệu: <b>{{ DocItem.doc_code }}</b>
                    | 
                    Ngày văn bản: <b >{{DocItem.doc_date}}</b>
                </div>
                <div class="doc-info">
                    {{DocItem.dispatch_book_code ? 'Số' + (DocItem.nav_type === 1 ? ' đến: ' : ' vào sổ: ') : ''}} 
                    <span v-if="DocItem.dispatch_book_code" v-html="'<b>' + DocItem.dispatch_book_code + '</b>'"></span>
                    <span v-if="DocItem.issue_place" v-html="' | Nơi ban hành: <b>' + DocItem.issue_place + '</b>'"></span>
                </div>
                <div v-if="DocItem.deadline_date_master" class="doc-deadline">
                  <span style="font-size:10px;"><i style="font-size: 11px;" class="pi pi-clock c-red-500"></i> Hạn văn bản: {{DocItem.deadline_date_master}}</span>
                </div>
                <div v-if="DocItem.date_deadline && DocItem.date_deadline > 3 && (DocItem.doc_status_id !== 'dadongdau' && DocItem.doc_status_id !== 'phanphat' && DocItem.doc_status_id !== 'hoanthanh')" class="follow-deadline">
                  <span style="font-size:10px;"><i style="font-size: 11px;" class="pi pi-flag c-red-500"></i> Hạn giao xử lý: {{DocItem.deadline_date}}</span>
                </div>
                <div v-if="props.Type === 'receive' && DocItem.date_deadline < 0 && (DocItem.status_id !== 'dadongdau' && DocItem.status_id !== 'phanphat' && DocItem.status_id !== 'hoanthanh')" class="count-deadline">
                  <Chip v-bind:label="'Quá hạn giao xử lý ' + DocItem.abs_date_deadline + ' ngày'" style="background-color: red; color: #fff; border-radius: 5px; font-size: 11px;" />
                  <!-- <Button type="button" :label="'Quá hạn xử lý ' + DocItem.abs_date_deadline + ' ngày'" class="p-button-danger" badgeClass="p-badge-danger" /> -->
                </div>
                <div v-if="props.Type === 'receive' && DocItem.date_deadline > 0 && DocItem.date_deadline <= 3 && (DocItem.status_id !== 'dadongdau' && DocItem.status_id !== 'phanphat' && DocItem.status_id !== 'hoanthanh')" class="count-deadline">
                  <Chip v-bind:label="'Hạn giao xử lý còn ' + DocItem.abs_date_deadline + ' ngày'" style="background-color: #ffc107; color: #fff; border-radius: 5px; font-size: 11px;" />
                  <!-- <Button type="button" :label="'Quá hạn xử lý ' + DocItem.abs_date_deadline + ' ngày'" class="p-button-danger" badgeClass="p-badge-danger" /> -->
                </div>
                <div v-if="props.Type === 'receive' && DocItem.date_deadline === 0 && (DocItem.status_id !== 'dadongdau' && DocItem.status_id !== 'phanphat' && DocItem.status_id !== 'hoanthanh')" class="count-deadline">
                  <Chip label="Đến hạn giao xử lý" style="background-color: #ffc107; color: #fff; border-radius: 5px; font-size: 11px;" />
                  <!-- <Button type="button" :label="'Quá hạn xử lý ' + DocItem.abs_date_deadline + ' ngày'" class="p-button-danger" badgeClass="p-badge-danger" /> -->
                </div>
                <!-- <Rating :modelValue="slotProps.data.rating" :readonly="true" :cancel="false"></Rating>
                <i class="pi pi-tag doc-category-icon"></i><span
                    class="doc-category">{{ slotProps.data.category }}</span> -->
            </div>
            <div class="doc-list-action">
                <div class="doc-date" :class="classStyleDoc">{{ DocItem.send_date }}</div>
                <div class="flex">
                  <div class="mr-1" v-if="DocItem.is_prioritized"><i style="color: orange" v-tooltip.top="'Ưu tiên'" class="pi pi-star"></i></div>
                  <div v-if="DocItem.countTask > 0"  @click="showModalListLinkTak()" v-tooltip.right="'Xem công việc liên kết'" class="mr-1" style="cursor: pointer"><i class="pi pi-link" style="font-size: 1.2rem"></i></div>
                  <DocType :TypeDoc="DocItem.nav_type"></DocType>
                </div>
                <DocStatus v-if="props.Type === 'receive'" :DocObj="DocItem"></DocStatus>
                <!-- <span class="doc-price">${{ slotProps.data.price }}</span>
                <Button icon="pi pi-shopping-cart" label="Add to Cart"
                    :disabled="slotProps.data.inventoryStatus === 'OUTOFSTOCK'"></Button>
                <span
                    :class="'doc-badge status-' + slotProps.data.inventoryStatus.toLowerCase()">{{ slotProps.data.inventoryStatus }}</span> -->
            </div>
        </div>
    </div>
    <DocLinkTask v-if="displayDialogListLinkTask === true" :id="DocItem.doc_master_id"
    :headerDialogList="headerDialogListLinkTask" :displayDialogList="displayDialogListLinkTask" :closeDialogList="CloseDiaLogListLinkTask" />
</template>

<style lang="scss" scoped>
::v-deep(.p-dataview) {
    .p-dataview.p-dataview-list .p-dataview-content > .p-grid > div {
    border: solid #dee2e6;
    border-width: 0 0 1px 0;
}
}
// .checkbox-radio{
//       display: none;
//       visibility: hidden;
//     }

//     .doc-list-item:hover .checkbox-radio{
//       display: block;
//       visibility: visible;
//     }
//     .doc-list-item:hover .avatar-image{
//       display: none;
//     }
.card {
    background: #ffffff;
    padding: 2rem;
    box-shadow: 0 2px 1px -1px rgba(0,0,0,.2), 0 1px 1px 0 rgba(0,0,0,.14), 0 1px 3px 0 rgba(0,0,0,.12);
    border-radius: 4px;
    margin-bottom: 2rem;
}
.p-dropdown {
    width: 14rem;
    font-weight: normal;
}

// .doc-name {
// 	font-size: 1.5rem;
// 	font-weight: 700;
// }

.doc-description {
	margin-top: 0.2rem;
    font-weight: 700;
}

.doc-info, .doc-deadline{
    margin-top: 0.2rem;
}

.doc-category-icon {
	vertical-align: middle;
	margin-right: .5rem;
}

.doc-category {
	font-weight: 600;
	vertical-align: middle;
}

.doc-not-file{
	color: #689f38;
}

.doc-not-seen{
	color: #278ddb;
	font-weight: 600;
}

.count-deadline{
  font-weight: 600 !important;
  color: red !important;
  margin-top: 0.2rem
}

.c-red-500{
    color: #f44336;
  }

::v-deep(.doc-list-item) {
	display: flex;
	align-items: center;
	padding: 0 1rem;
	width: 100%;

	img {
		width: 50px;
		// box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
		margin-right: 2rem;
	}

	.doc-list-detail {
		flex: 1 1 0;
	}

	.doc-list-action {
		display: flex;
		flex-direction: column;
        align-items: flex-end;
	}

	.p-button {
		margin-bottom: .5rem;
	}
}

::v-deep(.doc-grid-item) {
	margin: .5rem;
	border: 1px solid var(--surface-border);

	.doc-grid-item-top,
	.doc-grid-item-bottom {
		display: flex;
		align-items: center;
		justify-content: space-between;
	}

	img {
		box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
		margin: 2rem 0;
	}

	.doc-grid-item-content {
		text-align: center;
	}

	.doc-price {
		font-size: 1.5rem;
		font-weight: 600;
	}
}
.ava-text{
  margin-left: -1rem;
    margin-right: 1rem;
    background-color: rgb(33, 150, 243); 
    color: rgb(255, 255, 255);
}
@media screen and (max-width: 576px) {
	.doc-list-item {
		flex-direction: column;
		align-items: center;

		img {
			margin: 2rem 0;
		}

		.doc-list-detail {
			text-align: center;
		}

		.doc-price {
			align-self: center;
		}

		.doc-list-action {
			display: flex;
			flex-direction: column;
		}

		.doc-list-action {
			margin-top: 2rem;
			flex-direction: row;
			justify-content: space-between;
			align-items: center;
			width: 100%;
		}
	}
}
::v-deep(.p-avatar){
  background-color: unset;
}
</style>