<script setup>
    //Khai báo InJect và Import (import)
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

    //Khai báo biến (variable)
     const basedomainURL = baseURL;const toast = useToast();
const projectLogo = ref();
const projectSelected = ref();
const listProject = ref([]);
const listCategorySave = ref([]);
const listCategory = ref([]);
const database_name = ref();
const selectedKey = ref([]);
const expandedKeys = ref({});
const listCateSelected = ref([]);
const options = ref({
  IsNext: true,
  sort: "task_id",
  searchText: "",
  PageNo: 0,
  PageSize: 10,
  loading: true,
  totalRecords: null,
  finishedRecord: null,
  waitedRecord:null,
  tempClose: null,
  unFinishRecord: null,
  statusTask: null,
  outOfDate: null,
  SearchTextUser: "",
  Start_date: null,
  End_date: null,
});
const keyselected = ref(0);
const checkNode = ref(false);
const nodeValue = ref();
const categoryName = ref();
const nodeSelected = ref();const check_CateNull=ref();
const id_Khac=ref(1);
const isTypeAPI = ref(true);
const categoryIdSave = ref();
    // Khai báo hàm (function)

const loadProject = () => {
  (async () => {
    listProject.value = [];
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_project_list_api",
          par: [
            { par: "search", va: options.value.searchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];

        let db1 = {
          name: "Tất cả dự án",
          code: "allPr",
          db_name: null,
          project_logo: "/Portals/Image/noimg.jpg",
        };
        projectSelected.value = db1.code;
        emitter.emit("emitData", { type: "projectSelected",data:projectSelected.value});
        database_name.value = db1.db_name;
        projectLogo.value = db1.project_logo;
        listProject.value.push(db1);
        data.forEach((element) => {
          let db = {
            name: element.project_name,
            code: element.project_id,
            db_name: element.db_name,
            project_logo: element.project_logo,
          };
          listProject.value.push(db);
        });
      })
      .catch((error) => {
         
        options.value.loading = false;
        console.log(error);
        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });

    listCategory.value = [];
    listCategorySave.value = [];

    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_category_list",
          par: [
            { par: "parent_id", va: options.value.parent_id },
            {
              par: "project_id",
              va:
                projectSelected.value == "allPr" ? null : projectSelected.value,
            },
            { par: "search", va: options.value.searchText },
            { par: "status", va: options.value.Status },
            { par: "user_id", va: store.getters.user.user_id },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listCategorySave.value = data;
        emitter.emit("emitData", { type: "listCategorySave",data:data});
         
        renderCate(data);
        options.value.loading = false;
      })
      .catch((error) => {
        console.log(error);
         
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
          
          
  })();
};
const renderCate = (listCate) => {
  let arrCate = [];
  listProject.value.forEach((element) => {
    if (element.code == "allPr") return;
    else {
     
      let arrChils = [];
      listCate
        .filter((x) => x.parent_id == null && x.project_id == element.code)
        .forEach((m) => {
          let om = { key: m.category_id, data: m, count: m.taskcount };
          const rechildren = (mm, category_id) => {
            if (!mm.children) mm.children = [];
            if (!mm.count) mm.count = 0;
            let dts = listCate.filter((x) => x.parent_id == category_id);
            if (dts.length > 0) {
              dts.forEach((em) => {
                om.count += em.taskcount;
                
                let om1 = {
                  key: em.category_id,
                  data: em,
                  count: em.taskcount,
                };
                rechildren(om1, em.category_id);

                mm.children.push(om1);
              });
            }
          };

          rechildren(om, m.category_id);
          arrChils.push(om);
          
        });
        
      arrCate.push({
        key: element.code,
        data: element,
        count: null,
        children: arrChils,
      });
    }
    
    listCategory.value = arrCate;
      emitter.emit("emitData", { type: "listCategory",data:arrCate});
  });

  let arrReChild = [];
  listCate
    .filter((x) => x.parent_id == null)
    .forEach((m) => {
      let om = {
        key: m.category_id,
        label: m.category_name,
        data: m.category_id,
      };
      const rechildren = (mm, category_id) => {
        if (!mm.children) mm.children = [];
        let dts = listCate.filter((x) => x.parent_id == category_id);
        if (dts.length > 0) {
          dts.forEach((em) => {
            let om1 = {
              key: em.category_id,
              label: em.category_name,
              data: em.category_id,
            };
            rechildren(om1, em.category_id);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m.category_id);
      arrReChild.push(om);
    });
  listCateSelected.value = arrReChild;
    emitter.emit("emitData", { type: "listCateSelected",data:listCateSelected.value});
};

const renderTask = (listCate) => {
  let arrChils = [];

  listCate
    .filter((x) => x.parent_id == null)
    .forEach((m) => {
      let om = { key: m.category_id, data: m, count: m.taskcount };
      const rechildren = (mm, category_id) => {
        if (!mm.children) mm.children = [];
        if (!mm.count) mm.count = 0;
        let dts = listCate.filter((x) => x.parent_id == category_id);
        if (dts.length > 0) {
          dts.forEach((em) => {
            om.count += em.taskcount;

            let om1 = { key: em.category_id, data: em, count: em.taskcount };
            rechildren(om1, em.category_id);

            mm.children.push(om1);
          });
        }
      };
      
      rechildren(om, m.category_id);
      arrChils.push(om);
    });
    

  listCategory.value = arrChils;
  let arrReChild = [];
  listCate
    .filter((x) => x.parent_id == null)
    .forEach((m) => {
      let om = {
        key: m.category_id,
        label: m.category_name,
        data: m.category_id,
      };
      const rechildren = (mm, category_id) => {
        if (!mm.children) mm.children = [];
        let dts = listCate.filter((x) => x.parent_id == category_id);
        if (dts.length > 0) {
          dts.forEach((em) => {
            let om1 = {
              key: em.category_id,
              label: em.category_name,
              data: em.category_id,
            };
            rechildren(om1, em.category_id);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m.category_id);
      arrReChild.push(om);
    });
   
  listCateSelected.value = arrReChild;
  emitter.emit("emitData", { type: "listCateSelected",data:listCateSelected.value});
};
const loadCategory = () => {


emitter.emit("emitData", { type: "projectSelected",data:projectSelected.value});
  let atv = listProject.value.filter((x) => x.code == projectSelected.value)[0];
  if (atv) projectLogo.value = atv.project_logo;
  if (projectSelected.value == "allPr") {
    axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_category_list",
          par: [
            { par: "parent_id", va: options.value.parent_id },
            { par: "project_id", va: null },
            { par: "search", va: options.value.searchText },
            { par: "status", va: options.value.Status },
            { par: "user_id", va: store.getters.user.user_id },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];

        renderCate(data);

        options.value.loading = false;
      })
      .catch((error) => {
        console.log(error);
         
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  
  } else {
    
    (async () => {
      listCategorySave.value = [];
      await axios
        .post(
          baseURL + "/api/Proc/CallProc",
          {
            proc: "task_category_list",
            par: [
              { par: "parent_id", va: options.value.parent_id },
              { par: "project_id", va: projectSelected.value },
              { par: "search", va: options.value.searchText },
              { par: "status", va: options.value.Status },
              { par: "user_id", va: store.getters.user.user_id },
            ],
          },
          config
        )
        .then((response) => {
          let data = JSON.parse(response.data.data)[0];
          listCategorySave.value = data;
          renderTask(data);
          options.value.loading = false;
        })
        .catch((error) => {
          console.log(error);
           
          options.value.loading = false;

          if (error && error.status === 401) {
            swal.fire({
              title: "Error!",
              text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
              icon: "error",
              confirmButtonText: "OK",
            });
            store.commit("gologout");
          }
        });
    })();
  }
};

const refreshTypeApi = () => {

  options.value.loading = true;
  loadProject(true);
  onNodeSelect(nodeValue.value, false);
};

const onNodeSelect = (node, check) => {
  if(keyselected.value != node.key)
  {
    emitter.emit("emitData", { type: "loadCategory",data:node });
 
  keyselected.value = node.key;
  }
  if (selectedKey.value) {
    selectedKey.value[keyselected.value] = false;
    selectedKey.value[node.key] = true;
  }
  if (check) {
    if (expandedKeys.value[node.key] == true) {
      expandedKeys.value[node.key] = false;
    } else {
      expandedKeys.value[node.key] = true;
    }
  }
  nodeValue.value = node;

  options.value.loading = true;
  categoryName.value = node.data.category_name;

  if (node.data.category_id == null) {
    nodeSelected.value = null;

      check_CateNull.value=node.data;
let arrT=listCategorySave.value.filter((x)=>x.project_id== node.data.code && x.category_name=="Khác");
  if(arrT.length>0)
id_Khac.value=arrT[0].category_id;
    return;
  } else {
       check_CateNull.value=null;
    categoryIdSave.value = node.data.category_id;
    isTypeAPI.value = true;
    nodeSelected.value = node.data;
  }
};

const onUnNodeSelect = () => {
  check_CateNull.value=null;
};
onMounted(() => {
  loadProject(true);
  return {

  };
});
</script>
<template>
    <div>
        <div class="m-3 mr-0 flex">
          <div>
            <img
              :src="
                projectLogo
                  ? basedomainURL + projectLogo
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              "
              style="object-fit: contain"
              alt=""
              class="p-0 pr-2"
              width="45"
              height="30"
            />
         
          </div>
          <Dropdown
            v-model="projectSelected"
            :options="listProject"
            optionLabel="name"
            optionValue="code"
            placeholder="Chọn dự án"
            class="w-full"
            @change="loadCategory"
          >
          </Dropdown>
          <Button
            class="w-4rem ml-2 p-button-outlined p-button-secondary"
            icon="pi pi-refresh"
            @click="refreshTypeApi"
          />
        </div>

        <div style="height: calc(100vh - 128px)">
 
          <TreeTable
            :value="listCategory"
            @nodeSelect="onNodeSelect($event, true)"
            @node-unselect="onUnNodeSelect"
            selectionMode="single"
            v-model:selectionKeys="selectedKey"
            class="h-full w-full overflow-x-hidden"
            scrollHeight="flex"
            responsiveLayout="scroll"
            :scrollable="true"
            :expandedKeys="expandedKeys"
          >
            <Column
              field="category_name"
              :expander="true"
              class="cursor-pointer flex"
            >
              <template #header>
                <Toolbar class="w-full p-0 border-none ">
                  <template #start>
                    <div class="font-bold text-xl">Nhóm công việc</div>
                  </template>
                </Toolbar>
              </template>
              <template #body="data">
                <div
                  class="relative flex w-full p-0"
                  v-if="data.node.data.category_name"
                >
                  <div class="grid w-full p-0">
                    <div
                      class="field col-12 md:col-12 w-full flex m-0 p-0 pt-2"
                    >
                      <div class="col-2 p-0">
                        <img
                       
                          src="../../assets/image/folder.png"
                          width="28"
                          height="36"
                          style="object-fit: contain"
                          :style="data.node.count <= 0 ?'filter:grayscale(100%':''"
                        />
                      </div>
                      <div class="col-10 p-0">
                        <div class="px-2" style="line-height: 36px">
                          {{ data.node.data.category_name }}
                          <span v-if="data.node.count > 0"
                            >({{ data.node.count }})</span
                          >
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="relative flex w-full p-0" v-else>
                  <div class="grid w-full p-0">
                    <div
                      class="field col-12 md:col-12 w-full flex m-0 p-0 pt-2"
                    >
                      <div class="col-2 p-0">
                        <img
                          :src="
                            data.node.data.project_logo
                              ? basedomainURL + data.node.data.project_logo
                              : '/Portals/Image/noimg.jpg'
                          "
                          width="28"
                          height="36"
                          style="object-fit: contain"
                        />
                      </div>
                      <div class="col-9 p-0">
                        <div style="line-height: 36px">
                          {{ data.node.data.name }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Column>
          </TreeTable>
        </div>
    </div>
</template>

<style lang="scss" scoped>
::v-deep(.p-treetable) {
.p-treetable-thead{
 position: relative !important;
}
}
</style>