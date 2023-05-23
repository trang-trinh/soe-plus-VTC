<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
//const emitter = inject("emitter");
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const props = defineProps({
    headerDialog: String,
    displayDialog: Boolean,
    closeDialog: Function,
    modelApproved: Object,
    selectedNodes: Array,
    initDataApproved: Function
});
const files = ref([]);
const removeFile = (event) => {
  files.value = [];
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    files.value.push(element);
  });
};
const submitted = ref(false);
const approve = () => {
    submitted.value = true;
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    var obj = { ...props.modelApproved };
    var requests = props.selectedNodes.map((x) => x["request_id"]);

    let formData = new FormData();
    formData.append("is_type_approve", obj["is_type_approve"]);
    formData.append("content", obj["content"]);
    //formData.append("read_date", obj["read_date"]);
    for (var i = 0; i < files.length; i++) {
        let file = files[i];
        formData.append("files", file);
    }
    formData.append("requests", JSON.stringify(requests));
    axios
        .post(basedomainURL + "/api/request/Approved_Request", formData, config)
        .then((response) => {
            if (response.data.err === "1") {
                swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
                });
                return;
            }
            props.initDataApproved(true);
            swal.close();
            toast.success("Duyệt thành công!");
            props.closeDialog();
        })
        .catch((error) => {
            swal.close();
            swal.fire({
                title: "Thông báo!",
                text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                icon: "error",
                confirmButtonText: "OK",
            });
            return;
        });
    if (submitted.value) submitted.value = false;
};
</script>
<template>
    <Dialog
        :header="props.headerDialog"
        v-model:visible="props.displayDialog"
        :style="{ width: '40vw' }"
        :maximizable="false"
        :closable="false"
        :modal="true"
    >
        <form>
            <div class="grid formgrid m-2">
                <div class="col-12 md:col-12">
                    <div class="form-group flex mb-3" style="flex-direction: column;">
                        <label class="font-bold mb-2">Nội dung</label>
                        <Textarea
                            v-model="props.modelApproved.content"
                            :autoResize="true"
                            rows="4"
                            cols="30"
                        />
                    </div>
                </div>
                <div class="col-12 md-col-12">
                    <div class="form-group flex" style="flex-direction: column;">
                        <label class="font-bold mb-2">Tệp đính kèm</label>
                        <FileUpload
                            :multiple="false"
                            :fileLimit="1"
                            :show-upload-button="false"
                            :show-cancel-button="true"
                            @remove="removeFile"
                            @select="selectFile"
                            accept=""
                            choose-label="Chọn tệp"
                            cancel-label="Hủy"
                        >
                            <template #empty>
                                <p>Kéo thả tệp đính kèm vào đây.</p>
                            </template>
                        </FileUpload>
                    </div>
                </div>
            </div>
        </form>
        <template #footer>
            <Button
                label="Hủy"
                icon="pi pi-times"
                @click="props.closeDialog()"
                class="p-button-text"
            />
            <Button label="Gửi" icon="pi pi-send" @click="approve()" />
        </template>
    </Dialog>
</template>
<style scoped>
</style>