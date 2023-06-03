<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { de } from "date-fns/locale";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const toast = useToast();
const isDynamicSQL = ref(false);
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const options = ref({
  isNext: true,
  loading: false,
  user_id: store.getters.user.user_id,
  filter_organization_id: store.getters.user.organization_id,
  search: "",
  pageNo: 0,
  pageSize: 20,
  total: 0,
  sort: "full_name asc",
  orderBy: "asc",
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

//Get arguments
const props = defineProps({
  displayAddStore: Boolean,
  typeShare: Intl,
  DocSelected_ID: String,
  Doc_Compendium: String,
});

// Khai báo biến
var datalists = [];
const isFirst = ref(true);
const rootusers = ref([]);
const expandedKeys = ref({});
const ListFolder = ref([]);
const TreeData = ref([]);
const sttCate = ref(1);
const selectedKey = ref();
const folder_id_list = ref([]);
const expandedKeysFolder = ref({});
//watch chose
const selectedNodeUser = ref([]);

watch(selectedNodeUser, () => {
  changeUserChecked();
});

//Function choice
const choice = () => {
  emitter.emit("emitData", {
    type: "choiceusers",
    data: {
      submit: true,
      displayDialog: false,
      selected: rootusers.value.filter((x) => x.is_check),
    },
  });
};
const cancel = () => {
  props.value = {};
  emitter.emit("emitData", {
    type: "cancalShareFile",
    data: {
      submit: false,
      displayAddStore: false,
    },
  });
};
const RenderFolder = (ListTaskCategory) => {
  let List = [];
  ListTaskCategory.value
    .filter((c) => c.data.parent_id == null)
    .forEach((element, i) => {
      sttCate.value = sttCate.value + i + 1;

      let Cat = {
        key: element.data.folder_id,
        data: element.data,
        name: element.data.folder_name,
      };

      const RenderChild = (child, folder_id) => {
        if (!child.children) child.children = [];
        let listChilCate = ListTaskCategory.value.filter(
          (c) => c.data.parent_id == folder_id
        );
        listChilCate.forEach((element) => {
          let CatChild = {
            key: element.data.folder_id,
            data: element.data,
            name: element.data.folder_name,
          };
          if (!CatChild.children) CatChild.children = [];
          RenderChild(CatChild, element.data.folder_id);
          child.children.push(CatChild);
        });
      };
      RenderChild(Cat, element.data.folder_id);
      List.push(Cat);
    });
  TreeData.value = List;
};
const saveFolder = () => {
  var folder_id;
  if (props.typeShare == 2 && selectedKey.value) {
    folder_id = Object.keys(selectedKey.value);
  }
  else if(props.typeShare == 1 && datalists.length >0) {
    folder_id =datalists;
  }
  else {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn thư mục!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let file = {
    id_key: props.DocSelected_ID.toString(),
    folder_id: folder_id[0],
    file_name: props.Doc_Compendium,
    doc_type: props.typeShare,
  };
  let formData = new FormData();
  let ids =  folder_id.toString();
  formData.append("file", JSON.stringify(file));
  formData.append("ids", ids);
  axios({
    method: "post",
    url: baseURL + `/api/FileMain/Add_File_Doc`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        toast.success("Cập nhật thành công!");
        emitter.emit("emitData", {
          type: "cancalShareFile",
          data: {
            displayAddStore: false,
          },
        });
        swal.close();
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const initDoc = (id, type) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "doc_ca_folder_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "doc_master_id", va: id.toString() },
          { par: "type", va: type },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        if (data[0].length > 0) {
          data[0].forEach((element) => {
            if( data[1].map(x=> x.parent_id).includes(element.folder_id)) element.is_checked = true;
            let taskcategory = {
              key: element.folder_id,
              data: element,
              name: element.folder_name,
            };
            ListFolder.value.push(taskcategory);
          });
          RenderFolder(ListFolder);
          selectedKey.value = {};
          expandedKeysFolder.value = {};
          if (data[1].length > 0) {
            datalists = data[1].map(x => x.parent_id);
          let arr = [];
          data[1].forEach((item) => {
            selectedKey.value[item.parent_id] = true;
            if (item.path_id.length > 0)
              arr = arr.concat(item.path_id.slice(0, -1).split("/"));
          });
          folder_id_list.value = [...new Set(arr)]; // list phan tu khong trung nhau
        }
          if(folder_id_list.value.length>0){
            folder_id_list.value.forEach((item)=>{
              
              expandedKeysFolder.value[item] = true;
            })
          }
        }
      }
    })
    .catch((error) => {
    });
};
const onCheckBox = (u)=>{
  if (u.is_checked) {
      datalists.push(u.folder_id);
    } else {
      let idx = datalists.findIndex(u.folder_id);
      if (idx != -1) datalists.splice(idx, 1);
    }
}
onMounted(() => {
  initDoc(props.DocSelected_ID, props.typeShare);

  //initRender();
  return {};
});
</script>

<template>
  <Dialog
    :header="props.typeShare == 1? 'Coppy vào kho dữ liệu':'Link vào kho dữ liệu'
    "
    v-model:visible="props.displayAddStore"
    :style="{ width: '40vw' }"
    :modal="true"
    :closable="false"
    style="z-index: 999"
  >
    <form>
      <div class="grid formgrid m-2" style="max-height: calc(100vh - 200px)">
        <TreeTable
        v-if="props.typeShare== 1"
          :value="TreeData"
          :rowHover="true"
          @nodeSelect="ClickFolder"
          @nodeUnselect="unClickFolder"
          selectionMode="single"
          class="h-full w-full overflow-x-hidden p-0"
          scrollHeight="flex"
          responsiveLayout="scroll"
          :scrollable="true"
          :expandedKeys="expandedKeysFolder"
        >
          <Column
            field="name"
            :expander="true"
            class="cursor-pointer flex"
            style="height: 50px"
          >
            <template #body="data">
              <div class="relative flex w-full p-0" v-if="data.node.data">
                <div class="grid w-full p-0">
                  <div class="field col-12 md:col-12 w-full flex m-0 p-0 pt-2">
                    <div class="format-center mr-2">
                      <Checkbox
                      :binary="true"
                      v-model="data.node.data.is_checked"
                      @change="onCheckBox(data.node.data)"
                    />
                    </div>
                    <div class="col-1 p-0">
                      <img
                        :src="
                        basedomainURL + '/Portals/file/folder.png'
                        "
                        height="30"
                        style="object-fit: contain; margin-top: 2px;"
                      />
                    </div>
                    <div class="col-10 p-0 flex" style="align-items: center">
                      <div class="px-2" style="line-height: 20px">
                        {{ data.node.name }}
                        <span v-if="data.node.children.length > 0"
                          >({{ data.node.children.length }})</span
                        >
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </template>
          </Column>
        </TreeTable>
        <TreeTable
        v-if="props.typeShare== 2"
          :value="TreeData"
          :rowHover="true"
          @nodeSelect="ClickFolder"
          @nodeUnselect="unClickFolder"
          selectionMode="single"
          v-model:selectionKeys="selectedKey"
          class="h-full w-full overflow-x-hidden p-0"
          scrollHeight="flex"
          responsiveLayout="scroll"
          :scrollable="true"
          :expandedKeys="expandedKeysFolder"
        >
          <Column
            field="name"
            :expander="true"
            class="cursor-pointer flex"
            style="height: 50px"
          >
            <template #body="data">
              <div class="relative flex w-full p-0" v-if="data.node.data">
                <div class="grid w-full p-0">
                  <div class="field col-12 md:col-12 w-full flex m-0 p-0 pt-2">                   
                    <div class="col-1 p-0">
                      <img
                        :src="
                        basedomainURL + '/Portals/file/folder.png'
                        "
                        height="30"
                        style="object-fit: contain; margin-top: 2px;"
                      />
                    </div>
                    <div class="col-10 p-0 flex" style="align-items: center">
                      <div class="px-2" style="line-height: 20px">
                        {{ data.node.name }}
                        <span v-if="data.node.children.length > 0"
                          >({{ data.node.children.length }})</span
                        >
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </template>
          </Column>
        </TreeTable>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="cancel()"
        class="p-button-text"
      />
      <Button label="Chia sẻ" icon="pi pi-check" @click="saveFolder()" />
    </template>
  </Dialog>
</template>

<style scoped>
  .format-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}
</style>

<style lang="scss" scoped>
::v-deep(.p-treetable-thead) {
  tr > th {
    padding: 0 !important;
    height: 0 !important;
  }
}
</style>