<script setup>
import { ref, defineProps, inject, onMounted } from "vue";
import { formatDate } from "../../util/function";
const props = defineProps({
    DocItem: Object
});
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = fileURL;
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
// Defined Variable
const isLoading = ref(false);
const DocItem = props.DocItem;
const messages = ref([]);
const loadMessages = (rf) => {
    if (rf) {
        messages.value = {};
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
                proc: "doc_follows_list_messages",
                par: [
                    { par: "doc_master_id", va: DocItem.doc_master_id },
                ],
            },
            config
        )
        .then((response) => {
            let data = JSON.parse(response.data.data)[0];
            debugger
            if (data.length > 0) {
                data.forEach(function (r) {
                    if (r.send_date) r.send_date = formatDate(r.send_date, 'datetime');
                    if (r.recall_date) r.recall_date = formatDate(r.recall_date, 'datetime');
                    if (r.docfiles) r.docfiles = JSON.parse(r.docfiles);
                });
                messages.value = data;
            }
            else{
                messages.value = [];
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
onMounted(() => {
    loadMessages(true)
    return {
        isLoading,
        DocItem,
        messages,
    };
});
</script>
<template>
    <div v-if="messages.length > 0" class="message-wrap mt-3">
        <div v-for="item in messages" :key="item.follow_id" class="flex mb-3">
            <div class="message-ava mr-3">
                <Avatar :label="item.avatar ? '' : item.send_by_name.split(' ').at(-1).substring(0, 1)"
                    :image="basedomainURL + item.avatar"
                    style="background-color: #2196f3; color: #ffffff; vertical-align: middle" size="large"
                    shape="circle" />
            </div>
            <div class="message-content">
                <Panel>
                    <template #header>
                        <b class="mr-2">{{ item.send_by_name }}</b> <span>{{ item.send_date }}</span>
                    </template>
                    <div>
                        <span class="message-text">{{ item.message }}</span>
                    </div>
                    <div v-if="item.is_recall" class="recall-message mt-3">
                        <div>Ngày thu hồi: {{ item.recall_date }}</div>
                        <div>Nội dung thu hồi: {{ item.recall_message }}</div>
                    </div>
                    <div class="message-file mt-3">
                        <div v-for="fi in item.docfiles" :key="fi.file_id" class="file-box">
                            <a v-tooltip.top="fi.file_name" :href="basedomainURL + fi.file_path" download class="w-full no-underline">
                                <img :src="basedomainURL + '/Portals/Image/file/'
                                    + fi.file_type + '.png'
                                " style="
                                  width: 40px;
                                  height: 40px;
                                  object-fit: contain;
                                " alt="" />
                                <div class="mt-2"><b class="file-title">{{fi.file_name}}</b></div>
                            </a>
                        </div>
                    </div>
                </Panel>
            </div>
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
            v-if="messages.length == 0"
          >
            <img src="../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
</template>
<style scoped lang="scss">
::v-deep(.p-panel){
    .p-panel-header{
        border: none;
        width: max-content;
    }
    .p-panel-content{
        border: none;
    }
}
.recall-message {
    color: red;
}

.message-text{
    white-space: pre-wrap;
}
.message-wrap{
    overflow-y: auto;
    max-height: calc(100vh - 12.5rem) !important;
}
.message-file{
    display: flex;
    column-gap: 1rem;
    flex-wrap: wrap;
    row-gap: 1rem;
}
.file-box{
    max-width: 200px;
    border: 1px solid aliceblue;
    background-color: aliceblue;
    padding: 1rem;
}
.file-box a{
    display: flex;
    flex-direction: column;
    align-items: center;
}
.file-title{
    color: #333;
    text-overflow: ellipsis;
    white-space: nowrap;
    max-width: 170px;
    overflow: hidden;
    display: inline-block;
}
.file-box:hover .file-title{
    color: #2196F3;
};
</style>