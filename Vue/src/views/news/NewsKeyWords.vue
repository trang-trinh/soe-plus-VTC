
<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const news = ref({
  is_order: 1,
  title: "",
  des: "",
  contents: "",
  image: "",
  is_hot: false,
  IsLang: store.getters.langid,
  Menu_ID: store.state.idNews,
  news_type: 0,
  key_words: "",
  IsWriter: "",
  start_date: "",
  end_date: null,
  status: false,
});
const toast = useToast();
const isFirst = ref(true);
const datalists = ref([]);
const isSaveNews = ref(false);
const router = inject("router");

const options = ref({
  IsNext: true,
  sort: "news_id DESC",
  search: "",
  pageno: 1,
  pagesize: 10,
  pageno1: 0,
  pagesize1: 15,
  pageno2: 0,
  pagesize2: 10,
  loading: true,
  totalRecords: null,
  totalNotiRecords: null,
});

const keywords = ref(
  window.location.href.substring(
    window.location.href.lastIndexOf("orient-") + 7
  )
);
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "news_main_count",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "category_id", va: options.value.category_id },

          { par: "news_type", va: 0 },
          { par: "key_words", va: keywords.value.replace("-", " ") },
          { par: "search", va: options.value.search },
          { par: "status", va: 2 },
          { par: "is_hot", va: options.value.is_hot },
          { par: "is_notify", va: options.value.is_notify },

          { par: "start_date", va: options.value.start_date },
          { par: "end_date", va: options.value.end_date },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords - 11;
      } else options.value.totalRecords = 0;
    })
    .catch((error) => {
      console.log(error);
    });
};
const loadData = () => {
  loadCount();
  datalists.value = [];

  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "news_main_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "category_id", va: options.value.category_id },

          { par: "news_type", va: 0 },
          { par: "key_words", va: keywords.value.replace("-", " ") },
          { par: "search", va: options.value.search },
          { par: "status", va: 2 },
          { par: "is_hot", va: options.value.is_hot },
          { par: "is_notify", va: options.value.is_notify },
          { par: "datefilter", va: 0 },
          { par: "pageno", va: 0 },
          { par: "pagesize", va: 10 },
          { par: "start_date", va: options.value.start_date },
          { par: "end_date", va: options.value.end_date },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        // relateNew.value = [];
        datalists.value = [];
        data.forEach((element, i) => {
          element.is_order = i + 1;

          // relateNew.value.push({
          //   name: element.title,
          //   code: element.news_id,
          // });

          if (!element.created_date_show) element.created_date_show = null;
          element.created_date_show = moment(
            new Date(element.created_date)
          ).format("DD/MM/YYYY");
          if (!element.start_date_show) element.start_date_show = null;
          element.start_date_show = moment(new Date(element.start_date)).format(
            "DD/MM/YYYY hh:mm:ss"
          );
          if (element.news_id != news.value.news_id && i < 10)
            datalists.value.push(element);
        });
      } else {
        datalists.value = [];
      }

      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);

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
const showDetails = (data) => {
  localStorage.setItem("news", data.news_id);
  let srcMs = removeVietnameseTones(data.title);
  store.commit("setnews", data);
  if (router)
    router.push({
      path:
        "/news/direct/" +
        srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
        "-orient-" +
        data.news_id,
    });
};
const datalistViews = ref();
const onloadNewsViews = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "news_main_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "category_id", va: options.value.category_id },

          { par: "news_type", va: 0 },
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: 2 },
          { par: "is_hot", va: true },
          { par: "is_notify", va: options.value.is_notify },
          { par: "datefilter", va: 0 },
          { par: "pageno", va: options.value.pageno1 },
          { par: "pagesize", va: options.value.pagesize1 },
          { par: "start_date", va: options.value.start_date },
          { par: "end_date", va: options.value.end_date },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        datalistViews.value = data;
      }
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
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
  loadData();
  onloadNewsViews();
  return {
    options,
  };
});
</script><template>
  <div class="grid w-full mt-2">
    <div class="col-12 p-0 flex">
      <div class="surface-0col-9 pr-0 pb-0 pt-2">
        <div
          class="
            col-12
            pt-3
            pb-3
            surface-0
            text-4xl
            font-bold
            align-center
            format-center
            flex
          "
        >
          KẾT QUẢ TÌM KIẾM
        </div>
        <div class="col-12 p-0 surface-0" id="scrollTop1">
          <DataView
            class="w-full h-full e-sm"
            @page="onPage($event)"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink "
            responsiveLayout="scroll"
            scrollHeight="flex"
            :scrollable="true"
            layout="list"
            :lazy="true"
            :value="datalists"
            :loading="options.loading"
            :paginator="options.totalRecords > options.pagesize"
            :rows="options.pagesize"
            :totalRecords="options.totalRecords"
            :rowHover="true"
            :showGridlines="true"
            :pageLinkSize="options.pagesize"
            currentPageReportTemplate=""
          >
            <template #list="slotProps">
              <div class="grid p-0 w-full">
                <div class="col-12 px-3">
                  <div class="col-12">
                    <div
                      class="
                        font-bold
                        text-color-secondary text-xl text-justify
                        cursor-pointer
                        text-3line
                        hyper-link
                      "
                      @click="showDetails(slotProps.data)"
                    >
                      {{ slotProps.data.title }}
                    </div>
                  </div>
                  <div class="col-12 flex">
                    <div
                      class="col-3 p-2 pl-0 pt-0 cursor-pointer"
                      @click="showDetails(slotProps.data)"
                    >
                      <img
                        class="w-full h-10rem"
                        style="border-radius: 5px; object-fit: cover"
                        :src="
                          slotProps.data.image
                            ? basedomainURL + slotProps.data.image
                            : basedomainURL + '/Portals/Image/noimg.jpg'
                        "
                      />
                    </div>
                    <div class="col-9 p-0" style="font-size: 14px">
                      <div
                        class="col-12 p-0 pb-2 text-500"
                        style="font-size: 12px"
                      >
                        <i class="pi pi-clock" style="font-size: 12px"></i>
                        {{
                          moment(new Date(slotProps.data.approved_date)).format(
                            "HH:mm DD/MM/YYYY"
                          )
                        }}
                      </div>
                      <div
                        class="col-12 p-0 text-justify line-height-3 text-5line"
                        style="font-size: 14px"
                      >
                        {{ slotProps.data.des }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </template>
            <template #empty>
              <div
                class="
                  align-items-center
                  justify-content-center
                  p-4
                  text-center
                "
              >
                <img src="../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </DataView>
        </div>
        <!-- <div v-else>
            <div
                  class="
                    align-items-center
                    justify-content-center
                    p-4
                    text-center
                  "
                 
                >
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
          </div> -->
      </div>

      <div class="surface-0 col-3 sticky top-0">
        <div class="col-12 p-0 field sticky top-0" v-if="news.news_type == 0">
          <div>
            <div class="col-12 p-0 field">
              <Panel class="p-panel-unset-bottom">
                <template #header>
                  <div class="pt-5 pb-3 surface-0 w-full">
                    <div
                      class="
                        surface-0
                        flex
                        text-2xl
                        font-bold
                        px-2
                        text-align-center
                        w-full
                        m-0
                      "
                      style="border-left: 3px solid #0078d4"
                    >
                      <i class="pi pi-image mx-2 text-2xl" /> Tin nổi bật
                    </div>
                    <div></div>
                  </div>
                </template>
                <div
                  class="overflow-design p-0"
                  style="height: calc(100vh - 150px)"
                >
                  <DataView
                    class="pr-0"
                    @page="onPage($event)"
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink "
                    responsiveLayout="scroll"
                    :scrollable="true"
                    layout="list"
                    :lazy="true"
                    :value="datalistViews"
                    :loading="options.loading"
                    :paginator="options.totalRecords > options.pagesize"
                    :rows="options.pagesize"
                    :totalRecords="options.totalRecords"
                    :rowHover="true"
                    :showGridlines="true"
                    :pageLinkSize="options.pagesize"
                    currentPageReportTemplate=""
                  >
                    <template #list="slotProps">
                      <div
                        class="grid p-0 py-2 w-full"
                        v-if="slotProps.data.news_id != news.news_id"
                      >
                        <div class="col-12 p-0">
                          <div class="col-12 p-0 pt-2 flex">
                            <div
                              class="col-3 p-2 pt-0 cursor-pointer"
                              @click="showDetails(slotProps.data)"
                            >
                              <img
                                class="w-full h-4rem"
                                style="border-radius: 5px; object-fit: cover"
                                :src="
                                  slotProps.data.image
                                    ? basedomainURL + slotProps.data.image
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                              />
                            </div>
                            <div
                              class="col-9 p-0 cursor-pointer"
                              @click="showDetails(slotProps.data)"
                            >
                              <div class="col-12 p-0 pb-1 pr-2">
                                <div
                                  class="
                                    font-bold
                                    text-color-secondary text-justify text-3line
                                    hyper-link
                                  "
                                >
                                  {{ slotProps.data.title }}
                                </div>
                              </div>
                              <div
                                class="col-12 pb-0 pl-0 text-500 p-0"
                                style="font-size: 12px"
                              >
                                <i
                                  class="pi pi-clock"
                                  style="font-size: 12px"
                                ></i>
                                {{
                                  moment(
                                    new Date(slotProps.data.approved_date)
                                  ).format("HH:mm DD/MM/YYYY")
                                }}
                              </div>
                            </div>
                          </div>
                          <div class="col-12 pt-0">
                            <div
                              class="
                                text-justify text-3line
                                line-height-3
                                text-600
                              "
                              style="font-size: 12px"
                            >
                              {{ slotProps.data.des }}
                            </div>
                          </div>
                        </div>
                      </div>
                    </template>
                    <template #empty>
                      <div
                        class="
                          align-items-center
                          justify-content-center
                          p-4
                          text-center
                        "
                        v-if="!isFirst"
                      >
                        <img
                          src="../../assets/background/nodata.png"
                          height="144"
                        />
                        <h3 class="m-1">Không có dữ liệu</h3>
                      </div>
                    </template>
                  </DataView>
                </div>
              </Panel>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style  scoped>
.text-3line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 2;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.text-4line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 3;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.text-5line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 5;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.overflow-design {
  overflow-y: scroll;
}
.overflow-design::-webkit-scrollbar-thumb {
  background-color: white !important;
}
.overflow-design:hover::-webkit-scrollbar-thumb {
  background-color: #e0e0e0 !important ;
}
.hyper-link {
  color: black;
}
.hyper-link:hover {
  color: #1c80cf !important;
}
.d-del-comment {
  visibility: hidden;
}
.d-comment:hover .d-del-comment {
  visibility: visible;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel) {
  .p-panel-header {
    padding: 0;
    border: unset;
  }
  .p-panel-content {
    padding-right: 0px !important;
    border-bottom: unset !important;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel.p-panel-unset) {
  .p-panel-header {
    padding: 16px 8px;
    background-color: #fff !important;
  }
  .p-panel-content {
    padding: 0;
  }
}
::v-deep(.p-grid) {
  div {
    border-bottom: unset !important;
    border-width: 1px 0px 0px 0px !important;
  }
}
</style>