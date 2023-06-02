<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";

import moment from "moment";

//Khai báo

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const isFirst = ref(true);

const listKeyWords = ref([]);

const typeViewsCount = ref([
  {
    name: "Mới cập nhật",
    code: 1,
  },
  { name: "Xem nhiều", code: 2 },
  { name: "Mới nhất", code: 3 },
]);
const options = ref({
  IsNext: true,
  sort: "news_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  pageno1: 0,
  pagesize1: 15,
  pageno2: 0,
  pagesize2: 10,
  loading: true,
  totalRecords: null,
  totalNotiRecords: null,
  typeviews: 1,
  typetag: 1,
  datefilter: 1,
});
//Get arguments
const props = defineProps({
  keywords: String,
  display: Boolean,
});

const onPage = (event) => {
  options.value.pageno = event.page;
  options.value.pagesize = event.rows;
  loadData();
};

const loadData = (key) => {
  if (key) options.value.key_words = key;

  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "video_view_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "datefilter", va: options.value.datefilter },
          { par: "status", va: null },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach((item) => {
          if (!item.is_file_upload)
            item.thumbnail = get_youtube_thumbnail(item.link, "medium");
        });
        datalistVideos.value = data;
      } else {
        datalistVideos.value = [];
        isFirst.value = false;
      }
      options.value.totalRecords = JSON.parse(response.data.data)[1][0].c;
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//get thumbnail from ytb
function get_youtube_thumbnail(url, quality) {
  if (url) {
    var video_id, thumbnail, result;
    if ((result = url.match(/youtube\.com.*(\?v=|\/embed\/)(.{11})/))) {
      video_id = result.pop();
    } else if ((result = url.match(/youtu.be\/(.{11})/))) {
      video_id = result.pop();
    }

    if (video_id) {
      if (typeof quality == "undefined") {
        quality = "high";
      }

      var quality_key = "maxresdefault"; // Max quality
      if (quality == "low") {
        quality_key = "sddefault";
      } else if (quality == "medium") {
        quality_key = "mqdefault";
      } else if (quality == "high") {
        quality_key = "hqdefault";
      }

      var thumbnail =
        "http://img.youtube.com/vi/" + video_id + "/" + quality_key + ".jpg";
      return thumbnail;
    }
  }
  return false;
}
const datalistVideos = ref([]);
const showDetails = (data) => {
  localStorage.setItem("old_video", data.video_id);
  localStorage.setItem("video", data.video_id);
  let srcMs = removeVietnameseTones(data.title);
  store.commit("setvideo", data);
  location.href =
    "/news/videosview/" +
    srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
    "-orient-" +
    data.video_id;
};
const loadKeyWords = () => {
  listKeyWords.value = [
    {
      name: "Tất cả",
      code: "1",
      active: false,
    },
  ];
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "video_keywords_list",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let strs = "";
      data.forEach((element) => {
        if (element.key_words != null) {
          let arr = element.key_words.split(",");
          arr.forEach((element) => {
                   const item = { name: element, code: element, active: false };
            if (!strs.includes(element)) listKeyWords.value.push(item);
          });

          // for (let index = 0; index < arr.length; index++) {
          //   const item = { name: arr[index], code: arr[index], active: false };
          //   if (!strs.includes(arr[index])) listKeyWords.value.push(item);
          // }
          strs += element.key_words + ",";
        }
      });
    });
};
const removeSearch = () => {
  options.value.search = null;
  loadData();
};
const keyWordsFilter = (index, check) => {
  if (check) {
    listKeyWords.value
      .filter((x) => x.active == true)
      .forEach((element) => {
        element.active = false;
      });
    listKeyWords.value
      .filter((x) => x.code == options.value.key_words)
      .forEach((element) => {
        element.active = true;
      });
    loadData();
  } else {
    if (listKeyWords.value[index].active == false) {
      listKeyWords.value
        .filter((x) => x.active == true)
        .forEach((element) => {
          element.active = false;
        });
      options.value.key_words = listKeyWords.value[index].name;
      listKeyWords.value[index].active = true;
      loadData();
    } else {
      options.value.key_words = "1";
      listKeyWords.value[index].active = false;
      loadData();
    }
  }
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
function removeVietnameseTones(str) {
  str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
  str = str.replace(/đ/g, "d");
  str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
  str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
  str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
  str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
  str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
  str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
  str = str.replace(/Đ/g, "D");
  // Some system encode vietnamese combining accent as individual utf-8 characters
  // Một vài bộ encode coi các dấu mũ, dấu chữ như một kí tự riêng biệt nên thêm hai dòng này
  str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, ""); // ̀ ́ ̃ ̉ ̣  huyền, sắc, ngã, hỏi, nặng
  str = str.replace(/\u02C6|\u0306|\u031B/g, ""); // ˆ ̆ ̛  Â, Ê, Ă, Ơ, Ư
  // Remove extra spaces
  // Bỏ các khoảng trắng liền nhau
  str = str.replace(/ + /g, " ");
  str = str.trim();
  // Remove punctuations
  // Bỏ dấu câu, kí tự đặc biệt
  str = str.replace(
    /!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g,
    " "
  );
  return str;
}
onMounted(() => {
  loadData(props.keywords);
  loadKeyWords();

  return {
    options,
  };
});
</script>
<template>
  <div>
    <div class="flex w-full p-3">
      <div class="w-15rem mr-2">
        <Dropdown
          v-model="options.key_words"
          :options="listKeyWords"
          optionLabel="name"
          :filter="true"
          optionValue="code"
          placeholder="Từ khóa"
          class="w-full"
          @change="keyWordsFilter(null, true)"
        >
          <template #value="slotProps">
            <div
              class="p-dropdown-car-value flex text-justify"
              v-if="slotProps.value"
            >
              <div class="pr-2 text-justify">
                <i class="pi pi-sliders-h"></i>
              </div>
              <div class="text-justify flex format-center">
                {{ slotProps.value == "1" ? "Tất cả" : slotProps.value }}
              </div>
            </div>
            <span v-else>
              {{ slotProps.placeholder }}
            </span>
          </template></Dropdown
        >
      </div>
      <div class="w-15rem mr-2">
        <Dropdown
          v-model="options.datefilter"
          :options="typeViewsCount"
          optionLabel="name"
          optionValue="code"
          placeholder="Sắp xếp theo"
          class="w-full"
          @change="loadData()"
        >
          <template #value="slotProps">
            <div
              class="p-dropdown-car-value flex text-justify"
              v-if="slotProps.value"
            >
              <div class="pr-2 text-justify">
                <i class="pi pi-align-left"></i>
              </div>
              <div class="text-justify flex format-center">
                {{
                  slotProps.value == 1
                    ? "Mới cập nhật"
                    : slotProps.value == 2
                    ? "Xem nhiều"
                    : slotProps.value == 3
                    ? "Mới nhất"
                    : "Mới cập nhật"
                }}
              </div>
            </div>
            <span v-else>
              {{ slotProps.placeholder }}
            </span>
          </template></Dropdown
        >
      </div>
      <div class="w-full mr-2">
        <div class="w-full flex">
          <span class="w-full p-input-icon-left">
            <i class="pi pi-search" />
            <InputText
              type="text"
              v-model="options.search"
              spellcheck="false"
              @keyup.enter="loadData()"
              placeholder="Tìm kiếm"
            />
          </span>
          <Button
            v-if="options.search"
            @click="removeSearch"
            icon="pi pi-times"
            class="ml-2"
          ></Button>
        </div>
      </div>
      <!-- <div class="flex w-4 mr-2">
        <Chip
          v-for="(tag, index) in listTags"
          :key="index"
          class="h-full px-3 w-4 mr-2 format-center surface-0 cursor-pointer"
          >{{ tag.name }}</Chip
        >
        
      </div> -->
      <div class="flex mr-2" style="width: 600px; margin-bottom: 3px">
        <Chip
          @click="keyWordsFilter(1, false)"
          v-if="listKeyWords[1]"
          class="
            cursor-pointer
            h-full
            px-3
            w-4
            mr-2
            format-center
            surface-400
            text-0
            item-keywords
          "
          :style="
            listKeyWords[1].active == true
              ? 'background-color:#0078D4 !important'
              : ''
          "
          ><span>{{ listKeyWords[1].name }}</span></Chip
        >
        <Chip
          @click="keyWordsFilter(2, false)"
          v-if="listKeyWords[2]"
          class="
            cursor-pointer
            h-full
            px-3
            w-4
            mr-2
            format-center
            surface-400
            text-0
            item-keywords
          "
          :style="
            listKeyWords[2].active == true
              ? 'background-color:#0078D4 !important'
              : ''
          "
          ><span>{{ listKeyWords[2].name }}</span></Chip
        >
        <Chip
          @click="keyWordsFilter(3, false)"
          v-if="listKeyWords[3]"
          class="
            cursor-pointer
            h-full
            px-3
            w-4
            mr-2
            format-center
            surface-400
            text-0
            item-keywords
          "
          :style="
            listKeyWords[3].active == true
              ? 'background-color:#0078D4 !important'
              : ''
          "
          ><span>{{ listKeyWords[3].name }}</span></Chip
        >
      </div>
    </div>
    <!-- <div style="max-height: calc(100vh - 127px)"> -->
    <div>
      <!-- <DataView
        class="w-full h-full e-sm"
        responsiveLayout="scroll"
        :scrollable="true"
        layout="grid"
        :paginator="options.totalrecords > options.pagesize"
        :rows="options.pagesize"
        :totalRecords="options.totalrecords"
        :pageLinkSize="4"
          @page="onPage($event)"
        :lazy="true"
        :value="datalistVideos"
        :loading="options.loading"
        :rowHover="true"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        :rowsPerPageOptions="[12, 20, 50, 100, 200]"
        currentPageReportTemplate=""
      > -->
      <DataView
        class="w-full h-full e-sm"
        :lazy="true"
        :value="datalistVideos"
        layout="grid"
        :loading="options.loading"
        :paginator="true"
        :rows="options.pagesize"
        :totalRecords="options.totalRecords"
        :pageLinkSize="options.pagesize"
        @page="onPage($event)"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 32, 40, 52, 80]"
        currentPageReportTemplate=""
        responsiveLayout="scroll"
        :scrollable="false"
      >
        <template #grid="slotProps">
          <div style="padding: 0.65em" class="col-3 md:col-3 item-video">
            <!-- <Panel style="text-align: center">
          <h3>{{ slotProps.data.title }}</h3>
        </Panel> -->
            <Card
              class="no-paddcontent cursor-pointer"
              @click="showDetails(slotProps.data)"
            >
              <template #title>
                <div style="position: relative">
                  <!-- <Image
                  :src="
                    slotProps.data.image
                      ? basedomainURL + slotProps.data.image
                      : basedomainURL + '/Portals/Image/youtube.png'
                  "
                @error="
                $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
              "
                /> -->
                  <Image
                    :src="
                      slotProps.data.is_file_upload
                        ? slotProps.data.image
                          ? basedomainURL + slotProps.data.image
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                        : slotProps.data.image
                        ? basedomainURL + slotProps.data.image
                        : slotProps.data.thumbnail
                    "
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                  />
                  <span
                    v-if="slotProps.data.video_duration"
                    class="video-duration"
                    >{{ slotProps.data.video_duration }}</span
                  >
                </div>
              </template>
              <template #content>
                <div class="flex col-12 p-0">
                  <div class="px-0" style="flex: 2">
                    <Avatar
                      v-bind:label="
                        slotProps.data.avatar
                          ? ''
                          : slotProps.data.last_name.substring(0, 1)
                      "
                      v-bind:image="basedomainURL + slotProps.data.avatar"
                      style="background-color: #2196f3; color: #ffffff"
                      class="mr-2"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                  <div class="item-video" style="flex: 11">
                    <div>
                      <span class="font-bold text-2line">{{
                        slotProps.data.title
                      }}</span>
                    </div>
                    <div>
                      <span class="font-normal">{{
                        slotProps.data.created_name
                      }}</span>
                    </div>
                    <div>
                      {{ slotProps.data.is_visitor || 0 }} người xem |
                      {{
                        moment(new Date(slotProps.data.modified_date)).format(
                          "HH:mm DD/MM/YYYY"
                        )
                      }}
                    </div>
                  </div>
                </div>
              </template>
            </Card>
          </div>
        </template>
        <template #empty>
          <div
            class="
              align-items-center
              justify-content-center
              p-4
              text-center
              m-auto
            "
            v-if="!isFirst"
          >
            <img src="../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
      </DataView>
    </div>
  </div>
</template>
<style scoped>
.text-2line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 2;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.hyper-link {
  color: black;
}
.hyper-link:hover {
  color: #1c80cf !important;
}
.p-dataview-content {
  background: #f9f9f9 !important;
  max-height: calc(100vh - 127px);
  min-height: calc(100vh - 127px);
}
.video-duration {
  position: absolute;
  background: rgba(0, 0, 0, 0.4);
  font-size: 1rem;
  color: #fff;
  border-radius: 15%;
  padding: 1 3px;
  bottom: 10px;
  right: 10px;
  z-index: 99;
}
.bg-gray {
  background-color: #f9f9f9;
}
.item-video:hover {
  color: #1c80cf !important;
}
.item-keywords {
  background-color: #ececec !important;
  color: #030303 !important;
}
.item-keywords:hover {
  background-color: rgb(0 0 0 / 20%) !important;
  border: rgb(0 0 0 / 10%);
  transition: background-color 0.5s cubic-bezier(0.05, 0, 0, 1);
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel) {
  .p-panel-header {
    padding: 0;
    border: unset;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel.p-panel-unset) {
  .p-panel-header {
    padding: 12px 8px;
  }
  .p-panel-content {
    padding: 0 !important;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel.p-panel-unset-bottom) {
  .p-panel-content {
    padding-bottom: 0 !important;
  }
}
::v-deep(.p-dataview-content) {
  .p-card-body {
    padding: 0 !important;
  }
  .p-card-title img {
    width: 100%;
    height: 11.4vw;
    // object-fit: cover;
  }
  .p-card {
    box-shadow: none !important;
    background: #f9f9f9;
  }
  .p-card .p-card-title {
    margin-bottom: 0;
  }
  .p-avatar.p-avatar-xl {
    width: 3rem;
    height: 3rem;
    margin-top: 2px;
  }
}
::v-deep(.p-dataview) {
  .p-dataview-content {
    background: #f9f9f9 !important;
    max-height: calc(100vh - 152px);
    min-height: calc(100vh - 152px);
    margin-left: 5px;
  }
  .p-dataview .p-dataview-header {
    background: #f9f9f9 !important;
  }
}
::v-deep(.p-dataview-grid) {
  .p-dataview .p-dataview-header {
    background: #f9f9f9 !important;
  }
}
</style>


