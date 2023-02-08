    <script setup>
    import { ref, inject, onMounted } from "vue";
    import { useToast } from "vue-toastification";
    import { FilterMatchMode, FilterOperator } from "primevue/api";
    import moment from "moment";
    //Khai báo biến

    const modules = ref([
    { name: "Hệ thống", id: 1, parent_id: null},
    { name: "Danh sách tài khoản truy cập hệ thống", id: 2, parent_id: 1, link:'/history'},
    { name: "Văn bản", id: 3, parent_id: null},
    { name: "Báo cáo tổng hợp công văn đến", id: 4, parent_id: 3, link:'/doc/report/docreceive'},
    { name: "Báo báo tổng hợp công văn đi", id: 5, parent_id: 3, link: '/doc/report/docsend'},
    { name: "Báo cáo tổng hợp công văn nội bộ", id: 6, parent_id: 3, link:'/doc/report/docinternal'},
    { name: "Báo cáo tổng hợp công văn nghiệp vụ ", id: 7, parent_id: 3},
    { name: "Công việc", id: 8, parent_id: null},
    { name: "Báo cáo tổng hợp công việc của cá nhân", id: 9, parent_id: 8, link:'/tasks/report/task_personal'},
    { name: "Báo cáo tổng hợp công việc của phòng ban", id: 10, parent_id: 8, link:"/tasks/report/task_department"},
    // { name: "Báo cáo tổng hợp công việc của cả cơ quan", id: 11, parent_id:9 },
    { name: "Thiết bị", id: 12, parent_id: null},
    { name: "Báo cáo trang thiết bị công nghệ thông tin theo cá nhân", id:13 , parent_id: 12},
    { name: "Báo cáo trang thiết bị công nghệ thông tin theo phòng ban", id: 14, parent_id: 12},
    { name: "Báo cáo trang thiết bị công nghệ thông tin hết niên hạn", id: 15, parent_id: 12, link:'/device/report/device_expiration'},

    ]);
    const gotoModule = (item)=>{
        
    }
    onMounted(() => {
    //init
    return {
    };
    });
    </script>
    <template>
    <div class="w-full p-3 ">
        <h1 class="pt-0 mt-3 text-blue-700 format-center" >
          Báo cáo thống kê
        </h1>
        <div v-for="(item, index1) in modules.filter(x=>!x.parent_id)" :key="index1">
            <h2>
                <a href="">{{index1+1}}. {{item.name}}</a>
            </h2>
            <div class="flex">
                <div @click="gotoModule(child)" v-for="(child, index2) in modules.filter(x=>x.parent_id && x.parent_id == item.id)" :key="index2">
                <Chip
                      class="mx-3 px-3 py-1 cursor-pointer"
                      :label="child.name"
                      style="font-size: 16px !important;"
                    />
            </div>
            </div>
        </div>
    </div>
    </template>
