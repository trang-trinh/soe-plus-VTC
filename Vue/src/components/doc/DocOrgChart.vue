<script setup>
import { ref, defineProps, inject, onMounted } from "vue";
import {formatDate} from "../../util/function";
const props = defineProps({
    DocItem: Object,
    Type: String
});
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const basedomainURL = fileURL;
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
// Get emit
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "getRecallSelection":
        emitter.emit("emitData", { type: "returnRecallSelection", data: selection_nodes.value });;
        break;
    default:
        break;
  }
});
// Defined Variable
const isLoading = ref(false);
const DocItem = props.DocItem;
const follows = ref([]);
const follows_node = ref({});
const lstColorStatusFollow = {
    'fl-sohoa': {background_color: '#f2f4f6', text_color: '#72777a', doc_status_id: 'sohoa'},
    'fl-duthao': {background_color: '#ffc107', text_color: '#FFFFFF', doc_status_id: 'duthao'},
    'fl-xulychinh': {background_color: '#2196f3', text_color: '#FFFFFF', doc_status_id: 'xulychinh'},
    'fl-phoihop': {background_color: '#8BCFFB', text_color: '#FFFFFF', doc_status_id: 'phoihop'},
    'fl-xemdebiet': {background_color: '#CCADD7', text_color: '#FFFFFF', doc_status_id: 'xemdebiet'},
    'fl-phanphat': {background_color: '#CAE2B0', text_color: '#FFFFFF', doc_status_id: 'phanphat'},
    'fl-hoanthanh': {background_color: '#6dd230', text_color: '#FFFFFF', doc_status_id: 'hoanthanh'},
    'fl-tralai': {background_color: '#FF0000', text_color: '#FFFFFF', doc_status_id: 'tralai'},
    'fl-chopheduyet': {background_color: '#2196f3', text_color: '#FFFFFF', doc_status_id: 'chopheduyet'},
    'fl-chodongdau': {background_color: '#d87777', text_color: '#FFFFFF', doc_status_id: 'chodongdau'},
    'fl-dadongdau': {background_color: '#51b7ae', text_color: '#FFFFFF', doc_status_id: 'dadongdau'}
}
const generateTreeFollows = (par) => {
    var childs = follows.value.filter(x => x.follow_parent_id === par.follow_id);
    if (childs && childs.length > 0) {
        var childs_ord = childs.sort(function (a, b) {
            return new Date(b.send_date) - new Date(a.send_date);
        });
        // for (let i = 0; i < childs_ord.length; i++) {
        //     var obj = {
        //         follow_id: childs_ord[i].follow_id,
        //         key: par.key + '_' + i,
        //         type: 'docnode',
        //         styleClass: childs_ord[i].styleClass,
        //         data: { label: childs_ord[i].send_date, name: childs_ord[i].receive_by_name, avatar: childs_ord[i].avatar, background_color: childs_ord[i].background_color,  text_color: childs_ord[i].text_color
        //             , is_completed: childs_ord[i].is_completed, message: childs_ord[i].message, is_recall: childs_ord[i].is_recall},
        //         children: []
        //     }
        //     par.children.push(obj);
        //     generateTreeFollows(obj);
        // }
        if (childs_ord.length > 0) {
            childs_ord.forEach((elm,idx) => {
                var obj = {
                    follow_id: elm.follow_id,
                    key: par.key + '_' + idx,
                    type: 'docnode',
                    styleClass: elm.styleClass,
                    data: { label: elm.send_date, name: elm.receive_by_name, avatar: elm.avatar, background_color: elm.background_color,  
                        text_color: elm.text_color, is_completed: elm.is_completed, message: elm.message, is_recall: elm.is_recall},
                    children: []
                }
                par.children.push(obj);
                generateTreeFollows(obj);
            });
        }
    }
}
const loadFollows = (rf) => {
    if (rf) {
        follows_node.value = {};
        isLoading.value = true;
        swal.fire({
            width: 110,
            didOpen: () => {
                swal.showLoading();
            },
        });
        // if (options.value.PageNo == 1) loadCount();
    }
    axios
        .post(
            baseURL + "/api/Proc/CallProc",
            {
                proc: "doc_follows_list_follows_tree",
                par: [
                    { par: "doc_master_id", va: DocItem.doc_master_id },
                ],
            },
            config
        )
        .then((response) => {
            let data = JSON.parse(response.data.data)[0];
            if (data.length > 0) {
                debugger
                data.forEach(function (r) {
                    if (r.send_date) r.send_date = formatDate(r.send_date, 'datetime');
                    for(var key in lstColorStatusFollow){
                        if(lstColorStatusFollow[key].doc_status_id === r.doc_status_id && lstColorStatusFollow[key].background_color === r.background_color && lstColorStatusFollow[key].text_color === r.text_color){
                            r.styleClass = key;
                        }
                    }
                });
                follows.value = data;
                let init_node = JSON.parse(response.data.data)[1][0];
                if(init_node){
                    init_node.send_date = formatDate(init_node.send_date, 'datetime');
                    for(var key in lstColorStatusFollow){
                        if(lstColorStatusFollow[key].doc_status_id === init_node.doc_status_id && lstColorStatusFollow[key].background_color === init_node.background_color && lstColorStatusFollow[key].text_color === init_node.text_color){
                            init_node.styleClass = key;
                        }
                    }
                }
                follows_node.value = {
                    follow_id: init_node.follow_id,
                    key: 0,
                    type: 'docnode',
                    styleClass: init_node.styleClass,
                    data: { label: init_node.send_date, name: init_node.receive_by_name, avatar: init_node.avatar, is_recall: 0 },
                    children: []
                };
                generateTreeFollows(follows_node.value);
                setTimeout(function timeout() {
                    var container = document.querySelector(".p-organizationchart");
                    var middle = container.children[Math.floor((container.children.length - 1) / 2)];
                    container.scrollLeft = middle.offsetLeft +
                        middle.offsetWidth / 2 - container.offsetWidth / 2;
                }, 0)
            }
            if (rf) {
                isLoading.value = false;
                swal.close();
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
const loadRecallFollows = (rf) => {
    if (rf) {
        follows_node.value = {};
        isLoading.value = true;
        swal.fire({
            width: 110,
            didOpen: () => {
                swal.showLoading();
            },
        });
        // if (options.value.PageNo == 1) loadCount();
    }
    axios
        .post(
            baseURL + "/api/Proc/CallProc",
            {
                proc: "doc_follows_list_recall_tree",
                par: [
                    { par: "follow_id", va: DocItem.follow_id },
                ],
            },
            config
        )
        .then((response) => {
            let data = JSON.parse(response.data.data)[0];
            if (data.length > 0) {
                debugger
                data.forEach(function (r) {
                    if (r.send_date) r.send_date = formatDate(r.send_date, 'datetime');
                    for(var key in lstColorStatusFollow){
                        if(lstColorStatusFollow[key].doc_status_id === r.doc_status_id && lstColorStatusFollow[key].background_color === r.background_color && lstColorStatusFollow[key].text_color === r.text_color){
                            r.styleClass = key;
                        }
                    }
                });
                follows.value = data;
                let init_node = JSON.parse(response.data.data)[1][0];
                if(init_node){
                    init_node.send_date = formatDate(init_node.send_date, 'datetime');
                    for(var key in lstColorStatusFollow){
                        if(lstColorStatusFollow[key].doc_status_id === init_node.doc_status_id && lstColorStatusFollow[key].background_color === init_node.background_color && lstColorStatusFollow[key].text_color === init_node.text_color){
                            init_node.styleClass = key;
                        }
                    }
                }
                follows_node.value = {
                    follow_id: init_node.follow_id,
                    key: 0,
                    type: 'docnode',
                    styleClass: init_node.styleClass,
                    data: { label: init_node.send_date, name: init_node.receive_by_name, avatar: init_node.avatar },
                    children: []
                };
                generateTreeFollows(follows_node.value);
                setTimeout(function timeout() {
                    var container = document.querySelector(".p-organizationchart");
                    var middle = container.children[Math.floor((container.children.length - 1) / 2)];
                    container.scrollLeft = middle.offsetLeft +
                        middle.offsetWidth / 2 - container.offsetWidth / 2;
                }, 0)
            }
            if (rf) {
                isLoading.value = false;
                swal.close();
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
const onNodeSelect = (node) => {
    if(props.Type === 'recall'){
        if(node.key == 0){
            swal.fire({
            title: "Thông báo",
            text: "Không thể chọn người thu hồi gốc !",
            icon: "error",
            confirmButtonText: "OK",
        });
        }
        else{
            if(!node.is_checked){
                node.is_checked = true;
                selection_nodes.value.push(node);
            }
            else{
                node.is_checked = false;
                let idx = selection_nodes.value.findIndex(x=>x.key === node.key);
                if(idx > -1)
                selection_nodes.value.splice(idx,1);
            }
        }
    }
}
const resize_ob = new ResizeObserver(function (entries) {
  let width = entries[0].contentRect.width;
  // defaul width size item
  var sidebar = document.querySelector(".v-sidebar-menu");
  var left = document.querySelector("#splitter-doclist");
  var full_width = window.innerWidth || document.documentElement.clientWidth || 
document.body.clientWidth;
  if(left){
    left.style.width = ((40/100)*(full_width - sidebar.offsetWidth)) + "px";
  }
  var ele = document.querySelector(".auto-width");
  if(ele){
    // ele.style.width="calc(100vw - " + (sidebar.offsetWidth + width + 32) + "px)";
    ele.style.width = ((58/100)*(full_width - sidebar.offsetWidth) + 1) + "px";
  }
});
const selection_nodes = ref([]);
onMounted(() => {
     resize_ob.observe(document.querySelector("#splitter-doclist"));
    if(props.Type === 'all'){
        loadFollows(true);
    }
    else if(props.Type === 'recall'){
        loadRecallFollows(true);
    }
    return {
        follows_node,
        isLoading,
        DocItem,
        follows,
    };
});
</script>
<template>
    <OrganizationChart selectionMode="multiple" :value="follows_node" :collapsible="true" class="follow-doc">
        <template #docnode="slotProps">
            <div class="node-header ui-corner-top">{{slotProps.node.data.label}}</div>
            <div @click="onNodeSelect(slotProps.node)" :class="{'node-checked' : slotProps.node.is_checked}" class="node-content">
                <div v-if="slotProps.node.data.is_completed" style="position: absolute; right: 8px">
                    <i style="color: #6dd230" class="pi pi-check-circle"></i>
                </div>
                <div v-if="slotProps.node.data.is_recall" class="recall-item"><i class="pi pi-undo"></i></div>
                <div v-if="slotProps.node.data.message" class="message-box">{{slotProps.node.data.message}}</div>
                <Avatar
                v-bind:label="slotProps.node.data.avatar ? '' : slotProps.node.data.name.split(' ').at(-1).substring(0, 1)"
                v-bind:image="basedomainURL + slotProps.node.data.avatar"
                style="vertical-align: middle" size="small"
                shape="circle" />
                <div>{{slotProps.node.data.name}}</div>
            </div>
        </template>
    </OrganizationChart>
</template>
<style scoped lang="scss">
// ::v-deep(.p-organizationchart-table){
//     margin-top: 1rem !important
// }
.follow-doc{
    height: calc(100vh - 12rem);
    overflow-y: auto;
    padding: 1rem 0;
    overflow-x: auto;
    width: auto;
    white-space: nowrap;
    background-image: linear-gradient(90deg,rgba(200,0,0,.15) 10%,rgba(0,0,0,0) 10%),linear-gradient(rgba(200,0,0,.15) 10%,rgba(0,0,0,0) 10%);
    background-size: 10px 10px;
    border: 1px dashed transparent;
}
.follow-recall{
    max-width: 100%;
    height: 100%;
}
::v-deep(.p-organizationchart-table) {
    .p-doc-node {
        padding: 0;
        border: 0 none;
    }
    .p-organizationchart-line-top,
    .p-organizationchart-line-left,
    .p-organizationchart-line-right,
    .p-organizationchart-line-down{
        border-color: rgba(217,83,79,.8) !important;
        border-width: 2px !important;
    }
    .p-organizationchart-line-down{
        background: rgba(217,83,79,.8);
        width: 2px;
    }

    .p-organizationchart-node-content:hover .message-box{
        display: block;
    }

    .node-header, .node-content {
        padding: .5em .7rem;
    }

    .node-header{
        border-radius: 4px;
        border-bottom-left-radius: 0;
        border-bottom-right-radius: 0;
    }

    .node-content {
        text-align: center;
        border-radius: 4px;
        border-top-right-radius: 0;
        border-top-left-radius: 0;
    }

    .node-content.node-checked{
        background-color: orange;
    }

    .fl-sohoa .node-header,.fl-sohoa .node-content .p-avatar{
        background-color: #f2f4f6;
        color: #72777a;
    }

    .fl-sohoa .node-content{
        border: 1px solid #f2f4f6;
    }

    .fl-duthao .node-header,.fl-duthao .node-content .p-avatar{
        background-color: #ffc107;
        color: #FFFFFF;
    }

    .fl-duthao .node-content{
        border: 1px solid #ffc107;
    }

    .fl-xulychinh .node-header,.fl-xulychinh .node-content .p-avatar{
        background-color: #2196f3;
        color: #FFFFFF;
    }

    .fl-xulychinh .node-content{
        border: 1px solid #2196f3;
    }

    .fl-phoihop .node-header,.fl-phoihop .node-content .p-avatar{
        background-color: #8BCFFB;
        color: #FFFFFF;
    }

    .fl-phoihop .node-content{
        border: 1px solid #8BCFFB;
    }

    .fl-xemdebiet .node-header,.fl-xemdebiet .node-content .p-avatar{
        background-color: #CCADD7;
        color: #FFFFFF;
    }

    .fl-xemdebiet .node-content{
        border: 1px solid #CCADD7;
    }
    
    .fl-phanphat .node-header,.fl-phanphat .node-content .p-avatar{
        background-color: #CAE2B0;
        color: #FFFFFF;
    }

    .fl-phanphat .node-content{
       border: 1px solid #CAE2B0;
    }

    .fl-hoanthanh .node-header,.fl-hoanthanh .node-content .p-avatar{
        background-color: #6dd230;
        color: #FFFFFF;
    }

    .fl-hoanthanh .node-content{
        border: 1px solid #6dd230;
    }

    .fl-tralai .node-header,.fl-tralai .node-content .p-avatar{
        background-color: #FF0000;
        color: #FFFFFF;
    }

    .fl-tralai .node-content{
        border: 1px solid #FF0000;
    }

    .fl-chopheduyet .node-header,.fl-chopheduyet .node-content .p-avatar{
        background-color: #2196f3;
        color: #FFFFFF;
    }

    .fl-chopheduyet .node-content{
        border: 1px solid #2196f3;
    }

    .fl-chodongdau .node-header,.fl-chodongdau .node-content .p-avatar{
        background-color: #d87777;
        color: #FFFFFF;
    }

    .fl-chodongdau .node-content{
        border: 1px solid #d87777;
    }

    .fl-dadongdau .node-header,.fl-dadongdau .node-content .p-avatar{
        background-color: #51b7ae;
        color: #FFFFFF;
    }

    .fl-dadongdau .node-content{
        border: 1px solid #51b7ae;
    }

    .node-content img {
        border-radius: 50%;
    }
    .p-organizationchart-node-content{
        padding: 0;
        border: 0 none;
    }
    .message-box {
    display: none;
    position: absolute;
    top: -2rem;
    right: -14rem;
    z-index: 1;
    padding: 10px 14px;
    background: #f7ed7a;
    border-radius: 1.125rem 1.125rem 1.125rem 0;
    animation: fadeIn 0.3s ease-in;
    border: 1px solid #6dd230;
    width: 172px;
    white-space: pre-wrap;
}
.recall-item i{
    color: orange;
    font-size: 14px;
    position: absolute;
    right: 5px;
}
}
</style>