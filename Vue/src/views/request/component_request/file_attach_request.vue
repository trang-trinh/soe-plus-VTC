<script setup>
import { ref, inject, watch, onMounted } from "vue";
const emitter = inject("emitter");
const DataDetail = ref();
const props = defineProps({
    data: Object,
});
const ModalDetail = ref(false);
watch(ModalDetail, (vl) => {
    if (vl == false) {
        emitter.emit("closeViewFileRequest", false);
    }
});
const Documents2 = ref(
    "pdf,docx,docm,dotx,dotm,doc,dot,xlsx,xlsm,xltx,xltm,xlam,xlsb,xls,xlt,csv,tsv,pptx,pptm,potx,potm,ppsx,ppsm,ppt,pps,txt,xml,tif,tiff,svg,jpg,jpeg,jfif,png,gif,webp,bmp",
);
const Audios2 = ref("mp3,wma,aac,flac,alac,wav,m4a,flac");
const Videos2 = ref("mp4,flv,mov,wmv,avi,mkv,wmv,webm,h.264,mpeg-4");
const Documents1 = ref(
    ".pdf,.docx,.docm,.dotx,.dotm,.doc,.dot,.xlsx,.xlsm,.xltx,.xltm,.xlam,.xlsb,.xls,.xlt,.csv,.tsv,.pptx,.pptm,.potx,.potm,.ppsx,.ppsm,.ppt,.pps,.txt,.xml,.tif,.tiff,.svg,.jpg,.jpeg,.jfif,.png,.gif,.webp,.bmp",
);
const Audios1 = ref(".mp3,.wma,.aac,.flac,.alac,.wav,.m4a,.flac");
const Videos1 = ref(".mp4,.flv,.mov,.wmv,.avi,.mkv,.wmv,.webm,.h.264,.mpeg-4");
const Documents = ref();
const Audios = ref();
const Videos = ref();
const canViewFile = ref();
const basedomainURL = fileURL;
const Download = (file) => {
    var name = file.file_name || "file_download";
    const a = document.createElement("a");
    a.href = basedomainURL +
        "/Viewer/DownloadFile?url=" + file.file_path +
        "&title=" + name;
    a.download = name;
    a.click();
    a.remove();
};
const checkVideo = ref(false);
const checkMaximize = ref(false);
onMounted(() => {
    Documents.value = Documents1.value + "," + Documents2.value;
    Audios.value = Audios1.value + "," + Audios2.value;
    Videos.value = Videos1.value + "," + Videos2.value;
    canViewFile.value = Documents.value + "," + Audios.value + "," + Videos.value;
    canViewFile.value = canViewFile.value.split(",");
    Documents.value = Documents.value.split(",");
    Videos.value = Videos.value.split(",");
    Audios.value = Audios.value.split(",");
    if (
        canViewFile.value.includes(props.data.file_type) == true ||
        props.data.is_image == true
    ) {
        DataDetail.value = props.data;
        ModalDetail.value = true;
        checkVideo.value =
            Videos.value.includes(DataDetail.value.file_type.toLowerCase()) == true
                ? true
                : false;
    } else {
        Download(props.data);
        emitter.emit("closeViewFileRequest", false);
    }

    return;
});
</script>
<template>
    <Dialog v-model:visible="ModalDetail" 
        header="Nội dung file" 
        :modal="true" 
        :closable="true" 
        :style="{ width: '70vw' }"
        :maximizable="true" 
        :autoZIndex="true" 
        @maximize="checkMaximize = true" 
        @unmaximize="checkMaximize = false"
    >
        <div class="grid formgrid m-2 h-full format-center file-hover">
            <Image :src="basedomainURL + DataDetail.file_path" 
                :alt="DataDetail.file_name" 
                preview
                class="min-h-10rem format-center" 
                v-if="DataDetail.is_image == 1"
            ></Image>
            <video v-else-if="Videos.includes(DataDetail.file_type.toLowerCase()) == true" 
                style="width: 100%"
                :style="checkMaximize == false ? 'min-height:60vh' : 'height: 100%'" 
                controls
                :src="basedomainURL + DataDetail.file_path"
            ></video>
            <audio style="width: 100%; margin: 0px auto" 
                controls
                v-else-if="Audios.includes(DataDetail.file_type.toLowerCase()) == true"
            >
                <source :src="basedomainURL + DataDetail.file_path" />
            </audio>
            <iframe v-else-if="Documents.includes(DataDetail.file_type.toLowerCase()) == true" 
                allowfullscreen 
                :src="basedomainURL + '/Viewer/?title=' + DataDetail.file_name +
                        '&url=' + DataDetail.file_path" 
                style="width: 100%" 
                :style="checkMaximize == false ? 'min-height:70vh' : 'height: 100%'"
                title="Iframe file"
            >
            </iframe>
        </div>
    </Dialog>
</template>
<style scoped>
.format-center {
    display: flex;
    justify-content: center;
    align-items: center;
    vertical-align: middle;
    text-align: center;
}

.file-hover:hover {
    background-color: #d8edff;
}

.min-h-10rem {
    min-height: 10rem;
}
</style>
