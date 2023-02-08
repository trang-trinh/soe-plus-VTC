<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "primevue/usetoast";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import {   checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const selectedKey = ref();
const options = ref({
  SearchText: null,
  Status: null,
  loading: false,
  parent_id: null,
});
// eslint-disable-next-line no-undef
const basedomainURL = baseURL;
const ListProject = ref([]);
const SelectedProject = ref();
const SelectedProjectLogo = ref();
const LoadProject = () => {
  (async () => {
    await axios
      .post(
        basedomainURL + "/api/Proc/CallProc/",
        {
          proc: "api_project_list_api",
          par: [
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          let pj = {
            pj_id: element.project_id,
            pj_name: element.project_name,
            pj_logo: element.project_logo,
          };
          ListProject.value.push(pj);
        });
        SelectedProject.value = data[0].project_id;
        SelectedProjectLogo.value = data[0].project_logo;

        //noti:
      })
      .catch((error) => {
        options.value.loading = false;
        toast.add({
          severity: "error", //icon
          summary: "Lỗi", //noti
          detail: "Đã xảy ra lỗi khi tải dữ liệu", // mess
          life: 1000, //time
        });
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
};
const ReloadProjectList = () => {
  ListProject.value = [];
  LoadProject();
};
const ListTaskCategory = ref([]);
const expandedKeys = ref({});
const LoadTaskCategory = () => {
  options.value.project_id = SelectedProject.value;
  (async () => {
    await axios
      .post(
        basedomainURL + "/api/Proc/CallProc/",
        {
          proc: "task_category_list",
          par: [
            { par: "parent_id", va: options.value.parent_id },
            { par: "project_id", va: options.value.project_id },
            { par: "search", va: options.value.SearchText },
            { par: "status", va: options.value.Status },{ par: "user_id", va: store.getters.user.user_id},
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        ListTaskCategory.value = [];
        data.forEach((element) => {
          let taskcategory = {
            key: element.category_id,
            data: element,
            name: element.category_name,
          };
          ListTaskCategory.value.push(taskcategory);
        });
        
        RenderTree(ListTaskCategory);
      })
      .catch((error) => {
        options.value.loading = false;
        toast.add({
          severity: "error", //icon
          summary: "Lỗi", //noti
          detail: "Đã xảy ra lỗi khi tải dữ liệu", // mess
          life: 1000, //time
        });
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
};
const TreeData = ref();
const RenderTree = (ListTaskCategory) => {
  let List = [];
  ListTaskCategory.value
    .filter((c) => c.data.parent_id == null)
    .forEach((element,i) => {
           sttCate.value=  sttCate.value+i+1;
      
      let Cat = {
        key: element.data.category_id,
        data: element.data,
        name: element.data.category_name,
      };
   
      const RenderChild = (child, category_id) => {
        if (!child.children) child.children = [];
        let listChilCate = ListTaskCategory.value.filter(
          (c) => c.data.parent_id == category_id
        );
        listChilCate.forEach((element) => {
          let CatChild = {
            key: element.data.category_id,
            data: element.data,
            name: element.data.category_name,
          };
          if (!CatChild.children) CatChild.children = [];
          RenderChild(CatChild, element.data.category_id);
          child.children.push(CatChild);
        });
      };
      RenderChild(Cat, element.data.category_id);
      List.push(Cat);
    });
  TreeData.value = List;
};
const TreeTask = ref();
const RenderTreeTask = (ListTaskCategory, category_Id) => {
  TreeTask.value = [];
  let List = [];
  List.push({ key: 0, label: "Không có cha", data: null, children: [] });
  ListTaskCategory.value
    .filter(
      (c) => c.data.parent_id == null && c.data.category_id != category_Id
    )
    .forEach((element) => {
      let Cat = {
        key: element.data.category_id,
        label: element.data.category_name,
        data: element.data.category_id,
      };
      const RenderChild = (child, category_id) => {
        if (!child.children) child.children = [];
        let listChilCate = ListTaskCategory.value.filter(
          (c) =>
            c.data.parent_id == category_id && c.data.category_id != category_Id
        );
        listChilCate.forEach((element) => {
          let CatChild = {
            key: element.data.category_id,
            label: element.data.category_name,
            data: element.data.category_id,
          };

          RenderChild(CatChild, element.data.category_id);
          child.children.push(CatChild);
        });
      };
      RenderChild(Cat, element.data.category_id);
      List.push(Cat);
    });
  TreeTask.value = List;
};
const ReloadTree = () => {
  ListTaskCategory.value = [];
  LoadTaskCategory();
  RenderTree(ListTaskCategory);
};
const LoadData = () => {
  LoadProject();
  LoadTaskCategory();
};
const keyselected = ref();
const SelectedNode = ref();
const onNodeSelect = (node) => {
  task.value.project_id = SelectedProject.value;
  task.value.category_name = null;
  task.value.is_order = null;
  task.value.parent_id = null;
  isUpdate.value = false;
  SelectedNode.value = node.data;
  keyselected.value = node;
    sttCate.value=node.children.length+1;
      
  keyselected.value = node.key;
  if (selectedKey.value) {
    selectedKey.value[keyselected.value] = false;
    selectedKey.value[node.key] = true;
  }
  if (expandedKeys.value[node.key] == true) {
    expandedKeys.value[node.key] = false;
  } else {
    expandedKeys.value[node.key] = true;
  }
};
const onNodeUnselect = (node) => {
  temp.value = null;
  SelectedNode.value = null;
  isUpdate.value = false;
  task.value = taskDef.value;
  task.value.parent_id = null;
  expandedKeys.value[node.key] = false;
  task.value.project_id = SelectedProject.value;
};
const AddOrEditDialog = ref(false);
//khởi tạo
const task = ref({
  project_id: null,
  category_name: null,
  is_order: null,
  parent_id: null,
});
const taskDef = ref({
  project_id: null,
  category_name: null,
  is_order: null,
  parent_id: null,
});
const sttCate=ref(1);
const DiaglogHeader = ref(false);
const addTaskCategory = () => {
  if (SelectedNode.value != null) {
    task.value.parent_id = SelectedNode.value.category_id;
  } else {
    task.value = taskDef.value;
   
      task.value.status=true;
  }
   task.value.is_order=sttCate.value;
   task.value.project_id=SelectedProject.value;
  AddOrEditDialog.value = true;
  isUpdate.value = false;
  DiaglogHeader.value = true;
  DiaglogHeader.value = "Thêm danh mục";
};
const CloseDialog = () => {
  AddOrEditDialog.value = false;
  isUpdate.value = false;
  task.value = taskDef.value;
  submitted.value = false;
};
const isUpdate = ref(false);
const temp = ref();
const TempSelected = () => {
  for (var j in temp.value) {
    if (j == 0) task.value.parent_id = null;
    else task.value.parent_id = j;
  }
};
const UpdateTaskCategory = () => {
  isUpdate.value = true;
  AddOrEditDialog.value = true;
  task.value = SelectedNode.value;
  if (task.value.parent_id == null) {
    temp.value = { 0: true };
  } else {
    temp.value = { [task.value.parent_id]: true };
  }
  TreeTask.value = [];
  LoadTaskCategory();
  RenderTreeTask(ListTaskCategory, SelectedNode.value.category_id);
  DiaglogHeader.value = true;
  DiaglogHeader.value = "Sửa nhóm công việc";
};
const submitted = ref(false);
const rules = {
  category_name: {
    required,
    $errors: [
      {
        $property: "category_name",
        $validator: "required",
        $message: "Tên loại không được để trống!",
      },
    ],
  },
};
const v$ = useVuelidate(rules, task);
const Save = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  task.value.project_id = SelectedProject.value;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (isUpdate.value == false) {
    axios
      .post(
        basedomainURL + "/api/task_category/Add_Task_Category",
        task.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          task.value.category_name = null;
          task.value.is_order = null;
          task.value.parent_id = null;
          LoadTaskCategory();
          submitted.value = false;
          AddOrEditDialog.value = false;
          toast.add({
            severity: "success", //icon
            summary: "Thông báo", //noti
            detail: "Thêm thành công",
            life: 1000,
          });
        }
      })
      .catch(() => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    axios
      .post(
        basedomainURL + "/api/task_category/Update_Task_Category",
        task.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          task.value.category_name = null;
          task.value.is_order = null;
          task.value.parent_id = null;
          submitted.value = false;
          AddOrEditDialog.value = false;
          toast.add({
            severity: "success", //icon
            summary: "Thông báo", //noti
            detail: "Sửa thành công",
            life: 1000,
          });
          LoadTaskCategory();
        }
      })
      .catch(() => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};
const DeleteTaskCategory = (value) => {
  console.log(value);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm công việc này không ?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });

        axios
          .delete(basedomainURL + "/api/task_category/Delete_Task_Category", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value != null ? [value] : 0,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              SelectedNode.value = null;
              toast.add({
                severity: "success",
                summary: "Thông báo",
                detail: "Đã xóa nhóm công việc",
                life: 1000,
              });
              LoadTaskCategory();
            } else {
              swal.fire({
                title: "Error!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //  router.back();
  }
  LoadData();
});
</script>
<template>
  <div>

    <div class="w-full">
      <Splitter class="w-full">
        <SplitterPanel :size="20">
          <div class="m-3 mr-0 flex">
            <div>
              <img
                :src="
                  SelectedProjectLogo
                    ? basedomainURL + SelectedProjectLogo
                    : '../src/assets/image/noimg.jpg'
                "
                alt=""
                class="p-0 pr-2"
                width="45"
                height="40"
              />
            </div>
            <Dropdown
              v-model="SelectedProject"
              :options="ListProject"
              optionLabel="pj_name"
              optionValue="pj_id"
              placeholder="Chọn dự án"
              class="w-full"
              @change="ReloadTree(SelectedProject)"
            >
            </Dropdown>
            <Button
              class="w-4rem ml-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              @click="ReloadProjectList"
            />
          </div>
          <div style="height: calc(100vh - 128px)">
            <TreeTable
              :value="TreeData"
              @nodeSelect="onNodeSelect"
              @nodeUnselect="onNodeUnselect"
              selectionMode="single"
              v-model:selectionKeys="selectedKey"
              class="h-full w-full overflow-x-hidden p-0"
              scrollHeight="flex"
              responsiveLayout="scroll"
              :scrollable="true"
              :expandedKeys="expandedKeys"
            >
              <Column field="name" :expander="true" class="cursor-pointer flex">
                <template #header>
                  <Toolbar class="w-full p-0 border-none sticky top-0">
                    <template #start>
                      <div class="font-bold text-xl">Danh mục công việc</div>
                    </template>
                    <template #end>
                      <Button
                        icon="pi pi-plus "
                        class="p-button-success"
                        @click="addTaskCategory"
                      />
                      <div v-if="SelectedNode != null">
                        <Button
                          class="mx-1"
                          type="button"
                          icon="pi pi-pencil"
                          @click="UpdateTaskCategory"
                        ></Button>
                        <Button
                          icon="pi pi-trash"
                          class="p-button-danger"
                          @click="DeleteTaskCategory(SelectedNode.category_id)"
                        />
                      </div>
                    </template>
                  </Toolbar>
                </template>

                <template #body="data">
                  <div class="relative flex w-full p-0" v-if="data.node.data">
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
                          />
                        </div>
                        <div class="col-10 p-0">
                          <div class="px-2" style="line-height: 36px">
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
        </SplitterPanel>
        <SplitterPanel :size="70"><div>
          <Toolbar class="w-full">
                  <template #start>
                    <h3 class="m-0">
                      <i class="pi pi-book"></i> Thống kê công việc
                     
                    </h3>
                  </template>

                  <template #end>
                    
                  </template>
                </Toolbar>
        </div></SplitterPanel>
      </Splitter>
    </div>
    <div>
      <Dialog
        v-model:visible="AddOrEditDialog"
        :header="DiaglogHeader"
        :modal="true"
        :closable="true"
        class="p-fluid"
        style="width: 40vw"
      >
        <div clas="grid">
             <div v-if="isUpdate" class="flex col-12 md:col-12 lg:col-12">
            <div class="col-3">Cấp cha</div>
            <div class="col-9">
              <TreeSelect
                :options="TreeTask"
                spellcheck="false"
                selectionMode="single"
                v-model="temp"
                class="col-12 ip36 p-0"
                @change="TempSelected"
                placeholder="Chọn thư mục cha"
              />
            </div>
          </div>
          <div class="flex col-12 md:col-12 lg:col-12">
            <div class="col-3">Tên <span class="redsao">(*)</span></div>
            <div class="col-9">
              <InputText
                v-model.trim()="task.category_name"
                required="true"
                autofocus
                :class="{
                  'p-invalid': submitted && v$.category_name.$invalid,
                }"
              />
              <small
                v-if="
                  (v$.category_name.$invalid && submitted) ||
                  v$.category_name.$pending.$response
                "
                class="col-9 p-error p-0"
              >
                <span class="col-12 p-0">{{
                  v$.category_name.required.$message
                    .replace("Value", "Tên nhóm công việc")
                    .replace("is required", "không được để trống.")
                }}</span>
              </small>
            </div>
          </div>
          <div class="flex col-12 md:col-12 lg:col-12">
            <div class="col-3">Số thứ tự</div>
            <div class="col-3">
              <InputText v-model="task.is_order" />
            </div>
           <div class="col-6 flex pt-1">
            <label style="vertical-align: text-bottom" class="col-6 text-center"
              >Trạng thái
            </label>
            <InputSwitch v-model="task.status" class="col-6 ml-1" />
          </div>
          </div>
         

         
        </div>
        <template #footer>
          <Button
            label="Thoát"
            icon="pi pi-times"
            class="p-button-text"
            @click="CloseDialog"
          />
          <Button label="Lưu" icon="pi pi-check" @click="Save(!v$.$invalid)" />
        </template>
      </Dialog>
    </div>
  </div>
</template>
<style scoped></style>
<style lang="scss" scoped>
::v-deep(.p-treetable) {
  .p-treetable-tbody > tr > td {
    padding: 0;
  }
}
</style>
