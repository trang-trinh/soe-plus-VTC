<script setup>
import { ref, inject, watch, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import moment from "moment";
import { encr } from "../../../util/function.js";

const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const emitter = inject("emitter");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
  id: Intl,
  members: Array,
  type: Intl,
  data: Object,
  weight: Array,
  isClose: Boolean,
});
// const props = {
//   id: "FB45A50FA9FE4C618CD513BC1F2BD42A ",
//   members: [
//     {
//       member_id: "7333536D38E4469681C85F130467B7D1",
//       project_id: null,
//       task_id: "FB45A50FA9FE4C618CD513BC1F2BD42A",
//       user_id: "admin.bhbqp",
//       is_type: 0,
//       status: true,
//       is_view: true,
//       view_date: "2023-01-07T15:31:01.127",
//       created_by: "admin.bhbqp",
//       created_date: "2023-01-07T15:30:51.407",
//       created_ip: "171.237.237.140",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2023-01-07T15:30:51.407",
//       modified_ip: null,
//       modified_token_id: null,
//       full_name: "admin BHBQP",
//       avt: "/Portals/Users/9093911921578463637-512.png",
//       organiztion_name: "BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//       department_name: "Ban giám đốc",
//       positions: "Giám đốc",
//       STTGV: 0,
//       STTTH: null,
//       STTDTH: null,
//       STTTD: null,
//       tooltip: "Người giao việc<br/>admin BHBQP<br/>Giám đốc<br/>Ban giám đốc",
//     },
//     {
//       member_id: "3595DD25A5CF416FA7D00D0EE7A91CCF",
//       project_id: null,
//       task_id: "FB45A50FA9FE4C618CD513BC1F2BD42A",
//       user_id: "nvbqp07",
//       is_type: 1,
//       status: true,
//       is_view: null,
//       view_date: null,
//       created_by: "admin.bhbqp",
//       created_date: "2023-01-07T15:30:51.407",
//       created_ip: "171.237.237.140",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2023-01-07T15:30:51.407",
//       modified_ip: null,
//       modified_token_id: null,
//       full_name: "Tô Lan Hương",
//       avt: null,
//       organiztion_name: "BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//       department_name: "Phòng kế hoạch tổng hợp",
//       positions: "Nhân viên",
//       STTGV: null,
//       STTTH: 0,
//       STTDTH: null,
//       STTTD: null,
//       tooltip:
//         "Người xử lý chính<br/>Tô Lan Hương<br/>Nhân viên<br/>Phòng kế hoạch tổng hợp",
//     },
//     {
//       member_id: "39AA3767306E4C24AC563A517101B893",
//       project_id: null,
//       task_id: "FB45A50FA9FE4C618CD513BC1F2BD42A",
//       user_id: "hai.pham",
//       is_type: 1,
//       status: true,
//       is_view: null,
//       view_date: null,
//       created_by: "admin.bhbqp",
//       created_date: "2023-01-07T15:30:51.407",
//       created_ip: "171.237.237.140",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2023-01-07T15:30:51.407",
//       modified_ip: null,
//       modified_token_id: null,
//       full_name: "Phạm Thanh Hải BQP",
//       avt: "/Portals/Users/55897167_10205456613572072_4448588943213985792_n.jpg",
//       organiztion_name: "BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//       department_name: "BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//       positions: "Nhân viên",
//       STTGV: null,
//       STTTH: 1,
//       STTDTH: null,
//       STTTD: null,
//       tooltip:
//         "Người xử lý chính<br/>Phạm Thanh Hải BQP<br/>Nhân viên<br/>BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//     },
//     {
//       member_id: "A8A5A5D9AF704A48B769B53E163505AF",
//       project_id: null,
//       task_id: "FB45A50FA9FE4C618CD513BC1F2BD42A",
//       user_id: "admin.bhbqp",
//       is_type: 1,
//       status: true,
//       is_view: true,
//       view_date: "2023-01-07T15:31:01.127",
//       created_by: "admin.bhbqp",
//       created_date: "2023-01-07T15:30:51.407",
//       created_ip: "171.237.237.140",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2023-01-07T15:30:51.407",
//       modified_ip: null,
//       modified_token_id: null,
//       full_name: "admin BHBQP",
//       avt: "/Portals/Users/9093911921578463637-512.png",
//       organiztion_name: "BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//       department_name: "Ban giám đốc",
//       positions: "Giám đốc",
//       STTGV: null,
//       STTTH: 2,
//       STTDTH: null,
//       STTTD: null,
//       tooltip:
//         "Người xử lý chính<br/>admin BHBQP<br/>Giám đốc<br/>Ban giám đốc",
//     },
//     {
//       member_id: "A8DBBF09017C460CA7D2034557A5469B",
//       project_id: null,
//       task_id: "FB45A50FA9FE4C618CD513BC1F2BD42A",
//       user_id: "nvbqp05",
//       is_type: 1,
//       status: true,
//       is_view: null,
//       view_date: null,
//       created_by: "admin.bhbqp",
//       created_date: "2023-01-07T15:30:51.407",
//       created_ip: "171.237.237.140",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2023-01-07T15:30:51.407",
//       modified_ip: null,
//       modified_token_id: null,
//       full_name: "Phạm Lan Hương",
//       avt: "/Portals/Users/Screenshot_149118.png",
//       organiztion_name: "BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//       department_name: "Phòng kế hoạch tổng hợp",
//       positions: "Nhân viên",
//       STTGV: null,
//       STTTH: 3,
//       STTDTH: null,
//       STTTD: null,
//       tooltip:
//         "Người xử lý chính<br/>Phạm Lan Hương<br/>Nhân viên<br/>Phòng kế hoạch tổng hợp",
//     },
//     {
//       member_id: "11584157B4A54D4C9824D79188DC2A97",
//       project_id: null,
//       task_id: "FB45A50FA9FE4C618CD513BC1F2BD42A",
//       user_id: "nvbqp43",
//       is_type: 2,
//       status: true,
//       is_view: null,
//       view_date: null,
//       created_by: "admin.bhbqp",
//       created_date: "2023-01-07T15:30:51.407",
//       created_ip: "171.237.237.140",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2023-01-07T15:30:51.407",
//       modified_ip: null,
//       modified_token_id: null,
//       full_name: "Ngô Quỳnh Mai",
//       avt: null,
//       organiztion_name: "BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//       department_name: "Phòng công nghệ thông tin, hồ sơ",
//       positions: "Chánh văn phòng",
//       STTGV: null,
//       STTTH: null,
//       STTDTH: 0,
//       STTTD: null,
//       tooltip:
//         "Người đồng xử lý<br/>Ngô Quỳnh Mai<br/>Chánh văn phòng<br/>Phòng công nghệ thông tin, hồ sơ",
//     },
//     {
//       member_id: "5BD8C629C2C5417E862FB950A51FAC39",
//       project_id: null,
//       task_id: "FB45A50FA9FE4C618CD513BC1F2BD42A",
//       user_id: "admin.bhbqp",
//       is_type: 2,
//       status: true,
//       is_view: true,
//       view_date: "2023-01-07T15:31:01.127",
//       created_by: "admin.bhbqp",
//       created_date: "2023-01-07T15:30:51.407",
//       created_ip: "171.237.237.140",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2023-01-07T15:30:51.407",
//       modified_ip: null,
//       modified_token_id: null,
//       full_name: "admin BHBQP",
//       avt: "/Portals/Users/9093911921578463637-512.png",
//       organiztion_name: "BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//       department_name: "Ban giám đốc",
//       positions: "Giám đốc",
//       STTGV: null,
//       STTTH: null,
//       STTDTH: 1,
//       STTTD: null,
//       tooltip: "Người đồng xử lý<br/>admin BHBQP<br/>Giám đốc<br/>Ban giám đốc",
//     },
//     {
//       member_id: "81C9E2B26ADF4C0582FB24F95C0F61D3",
//       project_id: null,
//       task_id: "FB45A50FA9FE4C618CD513BC1F2BD42A",
//       user_id: "nvbqp01",
//       is_type: 2,
//       status: true,
//       is_view: null,
//       view_date: null,
//       created_by: "admin.bhbqp",
//       created_date: "2023-01-07T15:30:51.407",
//       created_ip: "171.237.237.140",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2023-01-07T15:30:51.407",
//       modified_ip: null,
//       modified_token_id: null,
//       full_name: "Trần Thị Minh Loan",
//       avt: "/Portals/9/Users/anh-dai-dien-dep-cho-zalo.jpeg",
//       organiztion_name: "BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//       department_name: "Ban giám đốc",
//       positions: "Giám đốc",
//       STTGV: null,
//       STTTH: null,
//       STTDTH: 2,
//       STTTD: null,
//       tooltip:
//         "Người đồng xử lý<br/>Trần Thị Minh Loan<br/>Giám đốc<br/>Ban giám đốc",
//     },
//     {
//       member_id: "4E4ECE01A9284B6DAE6C22D27E9A8012",
//       project_id: null,
//       task_id: "FB45A50FA9FE4C618CD513BC1F2BD42A",
//       user_id: "nvbqp26",
//       is_type: 3,
//       status: true,
//       is_view: null,
//       view_date: null,
//       created_by: "admin.bhbqp",
//       created_date: "2023-01-07T15:30:51.407",
//       created_ip: "171.237.237.140",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2023-01-07T15:30:51.407",
//       modified_ip: null,
//       modified_token_id: null,
//       full_name: "Đặng Thu Thủy",
//       avt: "/Portals/Users/92-928076_user-image-female-hd-png-download.png",
//       organiztion_name: "BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG",
//       department_name: "Văn thư",
//       positions: "Văn thư",
//       STTGV: null,
//       STTTH: null,
//       STTDTH: null,
//       STTTD: 0,
//       tooltip: "Người theo dõi<br/>Đặng Thu Thủy<br/>Văn thư<br/>Văn thư",
//     },
//   ],
//   type: 0,
//   data: {
//     task_id: "FB45A50FA9FE4C618CD513BC1F2BD42A",
//     task_code: null,
//     parent_id: null,
//     project_id: "745FA215E3F54195A2AA2BDB8C491154",
//     checklist_id: null,
//     is_check: null,
//     department_id: -1,
//     group_id: null,
//     task_name: "Test by Hoang pls no detele or change",
//     task_name_en: "test by hoang pls no detele or change",
//     description: [
//       "Dòng 1",
//       "Dòng 2",
//       "Dòng 3",
//       "Dòng 4",
//       "Dòng 5",
//       "Dòng 6",
//       "Dòng 7",
//     ],
//     keywords: null,
//     start_date: "2023-01-07T00:00:00",
//     end_date: "2023-01-31T00:00:00",
//     start_real_date: null,
//     end_real_date: null,
//     weight: 0,
//     difficult: [
//       "Dòng 1",
//       "Dòng 2",
//       "Dòng 3",
//       "Dòng 4",
//       "Dòng 5",
//       "Dòng 6",
//       "Dòng 7",
//     ],
//     target: [
//       "Dòng 1",
//       "Dòng 2",
//       "Dòng 3",
//       "Dòng 4",
//       "Dòng 5",
//       "Dòng 6",
//       "Dòng 7",
//     ],
//     result: null,
//     request: [
//       "Dòng 1",
//       "Dòng 2",
//       "Dòng 3",
//       "Dòng 4",
//       "Dòng 5",
//       "Dòng 6",
//       "Dòng 7",
//     ],
//     is_prioritize: true,
//     is_deadline: true,
//     is_review: true,
//     progress: 0,
//     is_todo: null,
//     is_public: null,
//     is_security: true,
//     close_by: null,
//     close_date: null,
//     finish_date: null,
//     status: 1,
//     is_order: null,
//     created_by: "admin.bhbqp",
//     created_date: "2023-01-07T13:46:26",
//     created_ip: "171.237.237.140",
//     created_token_id: null,
//     modified_by: "admin.bhbqp",
//     modified_date: "2023-01-07T15:30:51.393",
//     modified_ip: "171.237.237.140",
//     modified_token_id: null,
//     organization_id: 9,
//     project_name:
//       "Kỷ yếu 30 năm_Bài viết về các giai đoạn hình thành, phát triển của Trung tâm Tư vấn Thương mại quốc tế (ICCC)",
//     deparment_name: null,
//     group_name: null,
//     creator: "admin BHBQP",
//     statuss: "Đang làm",
//     bgColor: "#2196f3",
//     text_color: "#FFFFFF",
//     weights:
//       "<span style='color:#00cdff; font-weight: 700;margin-left: 5px;margin-right: 5px;'>Rất dễ</span>",
//   },
//   weight: [
//     {
//       weight_id: 13,
//       weight: 0,
//       weight_name: "Rất dễ",
//       progress: 100,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-02T09:52:08.397",
//       created_ip: "192.168.1.93",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2022-12-09T13:17:08.347",
//       modified_ip: "192.168.1.93",
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 14,
//       weight: 1,
//       weight_name: "Dễ",
//       progress: 75,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-02T09:52:32.513",
//       created_ip: "192.168.1.93",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2022-12-02T09:54:12.183",
//       modified_ip: "192.168.1.93",
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 15,
//       weight: 2,
//       weight_name: "Bình thường",
//       progress: 50,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-02T09:52:39.59",
//       created_ip: "192.168.1.93",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2022-12-02T09:54:17.737",
//       modified_ip: "192.168.1.93",
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 16,
//       weight: 4,
//       weight_name: "Hơi khó",
//       progress: 30,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-02T09:52:55.953",
//       created_ip: "192.168.1.93",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2022-12-02T09:54:25.09",
//       modified_ip: "192.168.1.93",
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 17,
//       weight: 5,
//       weight_name: "Khó",
//       progress: 25,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-02T09:53:01.993",
//       created_ip: "192.168.1.93",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2022-12-02T09:54:35.56",
//       modified_ip: "192.168.1.93",
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 18,
//       weight: 6,
//       weight_name: "Rất khó",
//       progress: 15,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-02T09:53:09.327",
//       created_ip: "192.168.1.93",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2022-12-02T09:54:43.64",
//       modified_ip: "192.168.1.93",
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 19,
//       weight: 7,
//       weight_name: "Không khả thi",
//       progress: 0,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-02T09:53:29.693",
//       created_ip: "192.168.1.93",
//       created_token_id: null,
//       modified_by: null,
//       modified_date: null,
//       modified_ip: null,
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 20,
//       weight: 8,
//       weight_name: "Không hiển thị",
//       progress: 0,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-02T16:12:37.05",
//       created_ip: "192.168.1.93",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2022-12-09T13:16:58.4",
//       modified_ip: "192.168.1.93",
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 21,
//       weight: 100,
//       weight_name: "okfwf",
//       progress: 0,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-03T10:39:16.03",
//       created_ip: "123.31.12.70",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2022-12-03T10:44:38.273",
//       modified_ip: "123.31.12.70",
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 24,
//       weight: 9,
//       weight_name: "ád",
//       progress: 0,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-12T09:35:00.673",
//       created_ip: "123.31.12.70",
//       created_token_id: null,
//       modified_by: "admin.bhbqp",
//       modified_date: "2022-12-12T09:42:43.123",
//       modified_ip: "123.31.12.70",
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 29,
//       weight: 3,
//       weight_name: "313fwsde",
//       progress: 0,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-13T09:17:16.413",
//       created_ip: "123.31.12.70",
//       created_token_id: null,
//       modified_by: null,
//       modified_date: null,
//       modified_ip: null,
//       modified_token_id: null,
//       organization_id: 9,
//     },
//     {
//       weight_id: 31,
//       weight: 44,
//       weight_name: "greg",
//       progress: 0,
//       status: true,
//       created_by: "admin.bhbqp",
//       created_date: "2022-12-14T15:05:21.553",
//       created_ip: "123.31.12.70",
//       created_token_id: null,
//       modified_by: null,
//       modified_date: null,
//       modified_ip: null,
//       modified_token_id: null,
//       organization_id: 9,
//     },
//   ],
// };
const basedomainURL = fileURL;
const countAll = ref();
const countChecked = ref();
const countUnChecked = ref();
const countChecklists = ref();
const ExpireTimeTask = ref();
const listCheckList = ref();
const listTaskCheckList = ref();

const loadTaskCheckList = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_checklist",
            par: [
              { par: "id", va: props.id },
              {
                par: "type",
                va: SelectedType.value,
              },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let listCheckListJson = JSON.parse(response.data.data)[0];
      let count = JSON.parse(response.data.data)[1];
      countChecklists.value = count[0].count;
      let count1 = JSON.parse(response.data.data)[2];
      let count2 = JSON.parse(response.data.data)[3];
      let count3 = JSON.parse(response.data.data)[4];
      countChecked.value = count1[0].countChecked ? count1[0].countChecked : 0;
      countUnChecked.value = count2[0].countUnChecked
        ? count2[0].countUnChecked
        : 0;
      ExpireTimeTask.value = count3[0].countExTime ? count3[0].countExTime : 0;
      let listChecklist = JSON.parse(response.data.data)[5];
      RenderData(listChecklist, listCheckListJson);
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công3!");
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const SelectedType = ref();
const RenderData = (cgr, c) => {
  listTaskCheckList.value = c;

  cgr.forEach((lcl) => {
    let i = 1;
    lcl.task = [];
    c.forEach((ltlc) => {
      ltlc.task_names = ltlc.task_name;
      ltlc.task_names = ltlc.task_names.replace(/\n/g, "<br/>");
      ltlc.actor_tooltip =
        "Người thực hiện <br/>" +
        ltlc.actor_full_name +
        "<br/>" +
        ltlc.actor_positions +
        "<br/>" +
        (ltlc.actor_department_name != null
          ? ltlc.actor_department_name
          : ltlc.actor_organiztion_name);
      ltlc.creator_tooltip =
        "Người tạo <br/>" +
        ltlc.creator_full_name +
        "<br/>" +
        ltlc.creator_positions +
        "<br/>" +
        (ltlc.creator_department_name != null
          ? ltlc.creator_department_name
          : ltlc.creator_organiztion_name);
      ltlc.workTime = Math.abs(
        (
          ((ltlc.end_date ? new Date(ltlc.close_date) : new Date()) -
            new Date(ltlc.start_date)) /
          (1000 * 24 * 60 * 60)
        ).toFixed(0),
      );
      if (ltlc.checklist_id == lcl.checklist_id) {
        ltlc.STT = i;
        i++;
        lcl.task.push(ltlc);
      }
    });
    lcl.totalRecords =
      SelectedType.value == 0 ? lcl.totalRecords : lcl.task.length;
  });
  progess.value = {
    prog:
      countChecked.value > 0
        ? ((countChecked.value / countChecklists.value) * 100).toFixed(0)
        : 0,
    tooltip:
      "Hoàn thành " + countChecked.value + "/" + countAll.value + " công việc",
  };
  listCheckList.value = [];
  listCheckList.value = cgr;
  loading.value = false;
};
const user = store.state.user;
const expandedRows = ref();

const DataVisible = ref(false);
watch(DataVisible, (vl) => {
  if (vl == false) {
    emitter.emit("closeTaskChecklists", false);
  }
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
const progess = ref({
  prog: null,
  tooltip: "",
});
const loadData = () => {
  loading.value = true;
  loadTaskCheckList();
};
const onCheckBox = (t) => {
  let data = {
    TextID: t.task_id,
  };
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .put(baseURL + "/api/CheckListDetail/Update_StatusTaskChecklist", data, {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái công việc thành công!");
        loadData();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html: ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const checkList = ref({
  checklist_id: null,
  project_id: null,
  task_id: null,
  checklist_name: null,
  description: null,
  status: null,
  is_order: null,
});
const rules = {
  checklist_name: {
    required,
  },
};
const v$ = useVuelidate(rules, checkList);
const headerDialog = ref();
const openDialog = ref();
const submitted = ref(false);
const edit = ref(false);
const addCheckList = () => {
  edit.value = false;
  checkList.value = {
    checklist_id: null,
    project_id: null,
    task_id: props.id,
    checklist_name: null,
    description: null,
    status: true,
    is_order: listCheckList.value.length + 1,
  };
  headerDialog.value = "Thêm mới Checklist";
  openDialog.value = true;
  submitted.value = false;
};
const editCheckList = (c) => {
  checkList.value = c;
  headerDialog.value = "Sửa checklist";
  openDialog.value = true;
  submitted.value = false;
  edit.value = true;
};
const closeDialog = () => {
  openDialog.value = false;
  openTaskDialog.value = false;
  loadData();
};
const textboxLength = ref();
const focusInput = () => {
  textboxLength.value = 0;
  const textbox = document.getElementById("textbox");
  textboxLength.value = textbox.value.length;
};
const saveData = (isFormValid) => {
  if (textboxLength.value > 500) {
    return;
  }
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: edit.value ? "put" : "post",
    url:
      baseURL +
      `/api/checkList/${edit.value ? "updateChecklist" : "addCheckList"}`,
    data: checkList.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        loadTaskCheckList();
        toast.success(
          response.config.method == "put"
            ? "Sửa Checklist thành công!"
            : "Thêm Checklist thành công",
        );
        swal.close();
        edit.value = false;
        submitted.value = false;
        openDialog.value = false;
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html:
            ms.includes("group_name") == true
              ? "Tên nhóm công việc không quá 250 ký tự!"
              : ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const deleteCheckList = (c) => {
  let id = [];
  id.push(c.checklist_id);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá Checklist này không!",
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
          .delete(baseURL + "/api/checkList/deleteChecklist", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: id,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá checklist thành công!");
              loadTaskCheckList();
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
const deleteMultipleCheckList = () => {
  let id = [];

  selectedGroup.value.forEach((x) => {
    id.push(x.checklist_id);
  });

  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá checklist này không!",
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
          .delete(baseURL + "/api/checkList/deleteTaskChecklist", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: id,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá công việc checklist thành công!");
              selectedGroup.value = [];
              loadData();
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

const scrollHeight = ref();
const selectedGroup = ref();
const selectedTask = ref();
watch(selectedGroup, (vl) => {
  if (selectedGroup.value.length > 0) {
    checkDelGroup.value = true;
  } else {
    checkDelGroup.value = false;
  }
});
watch(selectedTask, (vl) => {
  if (selectedTask.value.length > 0) {
    checkDelTask.value = true;
  } else {
    checkDelTask.value = false;
  }
});
const checkDelGroup = ref(false);
const checkDelTask = ref(false);
const loading = ref(true);
const Task = ref({
  STT: null,
  task_code: null,
  parent_id: null,
  project_id: null,
  checklist_id: null,
  department_id: null,
  group_id: null,
  task_name: null,
  task_name_en: null,
  description: null,
  keywords: null,
  start_date: null,
  end_date: null,
  start_real_date: null,
  end_real_date: null,
  weight: null,
  difficult: null,
  target: null,
  result: null,
  request: null,
  is_prioritize: null,
  is_deadline: null,
  is_review: null,
  progress: null,
  is_todo: null,
  is_public: null,
  is_security: null,
  close_by: null,
  close_date: null,
  status: null,
  is_order: null,
  organization_id: null,
  assign_user_id: "",
  work_user_ids: [],
  works_user_ids: [],
  follow_user_ids: [],
});
const rulesTaskCheckList = {
  task_name: {
    required,
  },
};
const v1$ = useVuelidate(rulesTaskCheckList, Task);
const headerTask = ref();
const submittedTask = ref(false);
const openTaskDialog = ref(false);
const addTaskCheckList = (c) => {
  Task.value.checklist_id = c.checklist_id;
  headerTask.value = "Thêm mới công việc checklist: " + c.checklist_name;
  openTaskDialog.value = true;
  Task.value = {
    checklist_id: c.checklist_id,
    start_date: new Date(),
    end_date: null,
    end_real_date: null,
    is_todo: true,
    is_check: false,
    is_order: c.totalRecords + 1,
    weight: props.weight[0].weight ?? null,
  };
};

const OpenViewTaskChecklists = (n) => {
  SelectedType.value = n;
  listCheckList.value = [];
  listTaskCheckList.value = [];
  loadData();
};
const editTask = ref(false);
const saveTask = (isFormValid) => {
  submittedTask.value = true;
  Task.value.parent_id = props.data.task_id;
  if (Task.value.end_real_date != null) Task.value.is_check = true;
  if (!isFormValid) {
    return;
  }
  if (Task.value.task_name.lastIndexOf("\n") == Task.value.task_name.length) {
    Task.value.task_name = Task.value.task_name.substring(
      0,
      Task.value.task_name.lastIndexOf("\n"),
    );
  }
  if (Task.value.end_date != null) {
    Task.value.is_deadline = true;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: editTask.value ? "put" : "post",
    url:
      baseURL +
      `/api/ChecklistDetail/${
        editTask.value ? "UpdateTaskChecklists" : "addTaskCheckList"
      }`,
    data: Task.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        closeDialog();
        toast.success(
          response.config.method == "put"
            ? "Cập nhật công việc thành công!"
            : "Thêm mới công việc thành công!",
        );
        if (response.config.method == "post") {
          SelectedType.value = 0;
        }
        loadData();
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html: ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

const editTaskCheckListFunc = (t) => {
  headerTask.value = "Sửa checklist";
  submittedTask.value = false;
  editTask.value = true;
  openTaskDialog.value = true;
  Task.value = t;
};
const deleteTaskCheckList = (id) => {
  let listID = [];
  listID.push(id);
  deleteTaskCheckListFunc(listID);
};
const deleteMultipleTaskChecklist = () => {
  let id = [];
  selectedTask.value.forEach((x) => {
    id.push(x.task_id);
  });
  deleteTaskCheckListFunc(id);
};
const deleteTaskCheckListFunc = (ids) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá công việc checklist này không!",
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
          .delete(baseURL + "/api/ChecklistDetail/deleteTaskChecklist", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá công việc checklist thành công!");
              selectedGroup.value = [];
              loadData();
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
const updateMultipleTask = () => {
  selectedTask.value
    .filter((x) => x.is_check != true)
    .forEach((t) => {
      let data = {
        TextID: t.task_id,
      };
      axios
        .put(
          baseURL + "/api/CheckListDetail/Update_StatusTaskChecklist",
          data,
          {
            headers: { Authorization: `Bearer ${store.getters.token}` },
          },
        )
        .then((response) => {
          if (response.data.err != "1") {
            swal.close();
          } else {
            let ms = response.data.ms;
            swal.fire({
              title: "Thông báo!",
              html: ms,
              icon: "error",
              confirmButtonText: "OK",
            });
          }
        })
        .catch((error) => {
          swal.close();
          swal.fire({
            title: "Thông báo",
            text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
            icon: "error",
            confirmButtonText: "OK",
          });
        });
    });
  setTimeout(() => {
    toast.success("Cập nhật trạng thái công việc thành công!");
    selectedTask.value = [];
    loadData();
  }, 500);
};
onMounted(() => {
  SelectedType.value = 0;
  loadData();
  setTimeout(() => {
    DataVisible.value = true;
  }, 500);
  scrollHeight.value = 50;
  return {};
});
</script>
<template>
  <Dialog
    v-model:visible="DataVisible"
    header="Checklist công việc"
    :modal="true"
    :breakpoints="{ '1366px': '100vw', '960px': '100vw', '640px': '100vw' }"
    :style="{ width: '75vw' }"
    :maximizable="true"
    :closable="true"
  >
    <form>
      <div
        class="col-12 p-0 m-0 flex py-1"
        style="font-weight: normal !important; font-size: 1rem"
      >
        <div class="col-8 p-0 m-0 left-0">
          <span>
            <span
              style="color: black"
              class="checklist-hover"
              :class="SelectedType == 0 ? 'activate1' : ''"
              @click="OpenViewTaskChecklists(0)"
              >Tất cả ({{ countChecklists }})
            </span>
            <span> | </span>
            <span
              style="color: #6dd230"
              class="checklist-hover"
              @click="OpenViewTaskChecklists(1)"
              :class="SelectedType == 1 ? 'activate1' : ''"
              >Đã check ({{ countChecked }})
            </span>
            <span> | </span>
            <span
              style="color: #ffa500"
              :class="SelectedType == 2 ? 'activate1' : ''"
              class="checklist-hover"
              @click="OpenViewTaskChecklists(2)"
            >
              Chưa check ({{ countUnChecked }})
            </span>
            <span> | </span>
            <span
              :class="SelectedType == 3 ? 'activate1' : ''"
              style="color: #ff0000"
              class="checklist-hover"
              @click="OpenViewTaskChecklists(3)"
            >
              Quá hạn ({{ ExpireTimeTask }})</span
            >
          </span>
        </div>
        <div
          class="col-4 p-0 m-0 right-0"
          style="text-align: right"
          v-if="isClose == false"
        >
          <Button
            v-if="checkDelGroup"
            label="xóa checklist"
            icon="pi pi-trash"
            class="p-button-danger p-0 m-0 px-4 py-1"
            @click="deleteMultipleCheckList()"
          />
          <Button
            label="Thêm mới checklist"
            icon="pi pi-plus"
            class="p-button-text p-0 m-0 px-4 py-1"
            @click="addCheckList()"
          />
        </div>
      </div>
      <DataTable
        :value="listCheckList"
        datakey="checklist_id"
        v-model:expandedRows="expandedRows"
        responsiveLayout="scroll"
        :scrollHeight="scrollHeight + 'vh'"
        v-model:selection="selectedGroup"
        :lazy="true"
        :loading="loading"
      >
        <template #header>
          <div
            class="col-12 flex format-center py-0 bgVar"
            style="--bgHeader: #ffffff"
          >
            <div class="col-2 format-center">
              <Avatar
                v-bind:label="
                  user.avatar
                    ? ''
                    : user.full_name.split(' ').at(-1).substring(0, 1)
                "
                v-bind:image="basedomainURL + user.avatar"
                style="color: #ffffff; cursor: pointer"
                :style="{
                  background: bgColor[1],
                  border: '2px solid' + bgColor[2],
                }"
                class="flex p-0 m-0"
                size="large"
                shape="circle"
              />
              <span class="p-4">{{ user.full_name }}</span>
            </div>
            <div class="col-2">
              <Knob
                v-model="progess.prog"
                :readonly="true"
                valueTemplate="{value}%"
                :valueColor="
                  progess.prog < 33
                    ? '#FF0000'
                    : progess.prog < 66
                    ? '#2196f3'
                    : '#6dd230'
                "
                :textColor="
                  progess.prog < 33
                    ? '#FF0000'
                    : progess.prog < 66
                    ? '#2196f3'
                    : '#6dd230'
                "
                v-tooltip.top="{ value: progess.tooltip }"
              />
            </div>
            <div class="col-6 flex format-center">
              <div class="col-12 format-center">
                <div
                  class="m-1 card col-3 format-default bgVar TextColor cursor-pointer"
                  style="
                    --bgHeader: #72777a;
                    --text: #ffffff;
                    min-width: fit-content;
                  "
                  :class="SelectedType == 0 ? 'activate2' : ''"
                  @click="OpenViewTaskChecklists(0)"
                >
                  <div class="col-12 p-0 m-0 w-full">
                    <div class="col-12 p-0 m-0">{{ countChecklists }}</div>
                    <div class="col-12 w-full p-0 m-0">Công việc</div>
                  </div>
                </div>
                <div
                  class="m-1 card col-3 format-default bgVar TextColor cursor-pointer"
                  style="
                    --bgHeader: #6dd230;
                    --text: #ffffff;
                    min-width: fit-content;
                  "
                  @click="OpenViewTaskChecklists(1)"
                  :class="SelectedType == 1 ? 'activate2' : ''"
                >
                  <div class="col-12 p-0 m-0 w-full">
                    <div class="col-12 p-0 m-0">{{ countChecked }}</div>
                    <div class="col-12 w-full p-0 m-0">Hoàn thành</div>
                  </div>
                </div>
                <div
                  class="m-1 card col-3 format-default bgVar TextColor cursor-pointer"
                  style="
                    --bgHeader: #ffa500;
                    --text: #ffffff;
                    min-width: fit-content;
                  "
                  :class="SelectedType == 2 ? 'activate2' : ''"
                  @click="OpenViewTaskChecklists(2)"
                >
                  <div class="col-12 p-0 m-0 w-full">
                    <div class="col-12 p-0 m-0">{{ countUnChecked }}</div>
                    <div class="col-12 w-full p-0 m-0">Chưa hoàn thành</div>
                  </div>
                </div>
                <div
                  class="m-1 card col-3 format-default bgVar TextColor cursor-pointer"
                  style="
                    --bgHeader: #ff0000;
                    --text: #ffffff;
                    min-width: fit-content;
                  "
                  :class="SelectedType == 3 ? 'activate2' : ''"
                  @click="OpenViewTaskChecklists(3)"
                >
                  <div class="col-12 p-0 m-0 w-full">
                    <div class="col-12 p-0 m-0">{{ ExpireTimeTask }}</div>
                    <div class="col-12 w-full p-0 m-0">Quá hạn</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>
        <template #expansion="slotProps">
          <div v-if="slotProps.data.totalRecords > 0">
            <div class="col-12 p-0 m-0 py-2 format-center flex">
              <h3 class="col-8 p-0 m-0 format-left">
                Danh sách công việc checklist:
                {{ slotProps.data.checklist_name }}
              </h3>
              <div
                class="col-4 p-0 m-0 format-right"
                v-if="isClose == false"
              >
                <Button
                  v-if="checkDelTask"
                  icon="pi pi-trash"
                  class="p-button-danger"
                  label="Xóa công việc"
                  @click="deleteMultipleTaskChecklist()"
                ></Button>
                <Button
                  v-if="checkDelTask"
                  icon="pi pi-check"
                  class="p-button-success"
                  label="Hoàn thành công việc"
                  @click="updateMultipleTask()"
                ></Button>
              </div>
            </div>

            <DataTable
              :value="slotProps.data.task"
              :dataKey="task_id"
              :rowHover="true"
              :showGridlines="true"
              responsiveLayout="scroll"
              :scrollable="true"
              :scrollHeight="'40vh'"
              v-model:selection="selectedTask"
            >
              <Column
                selectionMode="multiple"
                headerStyle="text-align:center;min-width:2rem;max-width:2rem"
                bodyStyle="text-align:center;min-width:2rem;max-width:2.15rem"
                class="align-items-center justify-content-center text-center"
              >
              </Column>

              <Column
                field="STT"
                header="STT"
                headerStyle="text-align:center;max-width:3rem;"
                bodyStyle="max-width:3rem;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span
                    :class="
                      slotProps.data.is_check == true
                        ? slotProps.data.thoigianquahan > 0
                          ? 'expired-task'
                          : 'checked-task'
                        : ''
                    "
                    >{{ slotProps.data.STT }}</span
                  >
                </template>
              </Column>
              <Column
                field="is_check"
                header="Trạng thái"
                headerStyle="text-align:center;max-width:7rem;"
                bodyStyle="max-width:7rem;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Checkbox
                    :binary="slotProps.data.is_check"
                    v-model="slotProps.data.is_check"
                    :disabled="isClose == fasle ? false : true"
                    @click="onCheckBox(slotProps.data)"
                  ></Checkbox
                ></template>
              </Column>
              <Column
                field="task_names"
                header="Tên công việc checklist"
                headerStyle=""
                bodyStyle=""
                class=""
              >
                <template #body="slotProps">
                  <span
                    :class="
                      slotProps.data.is_check == true
                        ? slotProps.data.thoigianquahan > 0
                          ? 'expired-task'
                          : 'checked-task'
                        : ''
                    "
                    v-tooltip="'Ưu tiên'"
                    v-if="slotProps.data.is_prioritize"
                    style="margin-right: 5px"
                    ><i
                      style="color: orange"
                      class="pi pi-star-fill"
                    ></i>
                  </span>
                  <span
                    :class="
                      slotProps.data.is_check == true
                        ? slotProps.data.thoigianquahan > 0
                          ? 'expired-task'
                          : 'checked-task'
                        : ''
                    "
                    v-html="slotProps.data.task_names"
                  ></span>
                </template>
              </Column>
              <Column
                field="created_by"
                header="Người giao việc"
                headerStyle="text-align:center;max-width:10rem;"
                bodyStyle="max-width:10rem;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span
                    class="flex format-center"
                    :class="
                      slotProps.data.is_check == true
                        ? slotProps.data.thoigianquahan > 0
                          ? 'expired-task'
                          : 'checked-task'
                        : ''
                    "
                  >
                    <Avatar
                      v-tooltip.right="{
                        value: slotProps.data.creator_tooltip,
                        escape: true,
                      }"
                      v-bind:label="
                        slotProps.data.creator_avt
                          ? ''
                          : slotProps.data.full_name
                              .split(' ')
                              .at(-1)
                              .substring(0, 1)
                      "
                      v-bind:image="basedomainURL + slotProps.data.creator_avt"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: bgColor[100 % 7],
                        border: '2px solid' + bgColor[200 % 7],
                      }"
                      class="flex p-0 m-0"
                      size="normal"
                      shape="circle"
                    />
                    <span class="pl-2">{{
                      slotProps.data.creator_full_name
                    }}</span>
                  </span>
                </template>
              </Column>
              <Column
                field="close_by"
                header="Người check"
                headerStyle="text-align:center;max-width:10rem;"
                bodyStyle="text-align:center;max-width:10rem;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span
                    class="flex format-center"
                    v-if="slotProps.data.close_by != null"
                    :class="
                      slotProps.data.is_check == true
                        ? slotProps.data.thoigianquahan > 0
                          ? 'expired-task'
                          : 'checked-task'
                        : ''
                    "
                  >
                    <Avatar
                      v-tooltip.right="{
                        value: slotProps.data.actor_tooltip,
                        escape: true,
                      }"
                      v-bind:label="
                        slotProps.data.avt
                          ? ''
                          : slotProps.data.actor_full_name
                              .split(' ')
                              .at(-1)
                              .substring(0, 1)
                      "
                      v-bind:image="basedomainURL + slotProps.data.avt"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: bgColor[100 % 7],
                        border: '2px solid' + bgColor[200 % 7],
                      }"
                      class="flex p-0 m-0"
                      size="normal"
                      shape="circle"
                    />
                    <span class="pl-2">{{
                      slotProps.data.actor_full_name
                    }}</span>
                  </span>
                </template>
              </Column>
              <Column
                field="start_date"
                header="Ngày bắt đầu"
                headerStyle="text-align:center;max-width:8.5rem;"
                bodyStyle="text-align:center; max-width:8.5rem"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="t">
                  <span
                    :class="
                      t.data.is_check == true
                        ? t.data.thoigianquahan > 0
                          ? 'expired-task'
                          : 'checked-task'
                        : ''
                    "
                  >
                    {{
                      moment(new Date(t.data.start_date)).format("DD/MM/YYYY")
                    }}</span
                  >
                </template>
              </Column>
              <Column
                field="end_date"
                header="Hạn xử lý"
                headerStyle="text-align:center;max-width:7rem;"
                bodyStyle="text-align:center;max-width:7rem"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="t">
                  <span
                    v-if="t.data.end_date"
                    :class="
                      t.data.is_check == true
                        ? t.data.thoigianquahan > 0
                          ? 'expired-task'
                          : 'checked-task'
                        : ''
                    "
                    >{{
                      moment(new Date(t.data.end_date)).format("DD/MM/YYYY")
                    }}</span
                  >
                </template>
              </Column>
              <Column
                field=""
                header="Số ngày làm"
                headerStyle="text-align:center;max-width:8rem;"
                bodyStyle="text-align:center;max-width:8rem"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="t">
                  <span
                    v-if="t.data.is_check"
                    :class="
                      t.data.is_check == true
                        ? t.data.thoigianquahan > 0
                          ? 'expired-task'
                          : 'checked-task'
                        : ''
                    "
                    >{{ t.data.workTime }}</span
                  >
                </template>
              </Column>
              <Column
                field="close_date"
                header="Hoàn thành"
                headerStyle="text-align:center;max-width:8rem;"
                bodyStyle="text-align:center;max-width:8rem;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="t">
                  <span
                    v-if="t.data.is_check"
                    :class="
                      t.data.is_check == true
                        ? t.data.thoigianquahan > 0
                          ? 'expired-task'
                          : 'checked-task'
                        : ''
                    "
                    >{{
                      moment(new Date(t.data.close_date)).format("DD/MM/YYYY")
                    }}</span
                  >
                </template>
              </Column>
              <Column
                field=""
                header="Chức năng"
                headerStyle="text-align:center;max-width:8rem;"
                bodyStyle="text-align:center;max-width:8rem;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="t">
                  <span
                    class="format-center"
                    v-if="t.data.is_check != true && isClose == false"
                  >
                    <Button
                      icon="pi pi-pencil font-bold"
                      class="p-button-rounded p-button-secondary p-button-outlined"
                      v-tooltip.top="{ value: 'Sửa checklist' }"
                      @click="editTaskCheckListFunc(t.data)"
                    />
                    <Button
                      icon="pi pi-trash font-bold"
                      class="p-button-rounded p-button-secondary p-button-outlined"
                      v-tooltip.top="{ value: 'Xóa công việc checklist' }"
                      @click="deleteTaskCheckList(t.data.task_id)"
                    />
                  </span>
                </template>
              </Column>
              <template #empty> </template>
            </DataTable>
          </div>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            style="display: flex; flex-direction: column"
            v-else
          >
            <img
              src="../../../assets/background/nodata.png"
              height="144"
            />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
        <Column
          :expander="true"
          headerStyle="width:3rem"
        />
        <Column
          selectionMode="multiple"
          headerStyle="text-align:center;width:2rem"
          bodyStyle="text-align:center;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="checklist_name"
          class="font-bold"
          header="Tên checklist"
        >
          <template #body="slotProps">
            <span>
              {{ slotProps.data.checklist_name }} ({{
                slotProps.data.totalRecords
              }})
            </span>
          </template>
        </Column>
        <Column
          field=""
          header="Chức năng"
          class="font-bold"
          headerStyle="width:10rem"
        >
          <template #body="slotProps">
            <span
              class="flex format-center"
              v-if="isClose == false"
            >
              <Button
                icon="pi pi-plus-circle font-bold "
                class="p-button-rounded p-button-secondary p-button-outlined"
                v-tooltip.top="{ value: 'Thêm công việc Checklist' }"
                @click="addTaskCheckList(slotProps.data)"
              />
              <Button
                icon="pi pi-pencil font-bold "
                class="p-button-rounded p-button-secondary p-button-outlined"
                v-tooltip.top="{ value: 'Sửa Checklist' }"
                @click="editCheckList(slotProps.data)"
              />
              <Button
                icon="pi pi-trash font-bold"
                class="p-button-rounded p-button-secondary p-button-outlined"
                v-tooltip.top="{ value: 'Xóa Checklist' }"
                @click="deleteCheckList(slotProps.data)"
              />
            </span>
            <Column
              :expander="true"
              headerStyle="width: 3rem"
            />
            <Column
              field="checklist_name"
              headerStyle="width: 3rem"
            />
          </template>
        </Column>
      </DataTable>
    </form>
    <template #footer>
      <Button
        label="Đóng"
        icon="pi pi-times"
        @click="DataVisible = false"
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerDialog"
    v-model:visible="openDialog"
    :style="{ width: '40vw' }"
    :closable="true"
    style="z-index: 10000"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 format-center">
          <div class="col-3 text-left p-0">
            Tên Checklist <span class="redsao">(*)</span>
          </div>
          <div class="col-9 p-0">
            <Textarea
              :autoResize="true"
              rows="2"
              id="textbox"
              v-model="checkList.checklist_name"
              spellcheck="false"
              class="w-full ip36 px-2"
              :class="{
                'p-invalid':
                  (v$.checklist_name.$invalid && submitted) ||
                  textboxLength > 500,
              }"
              scrollable="true"
              :scrollHeight="'10vh'"
              @change="focusInput()"
            />
          </div>
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12 px-0"
          v-if="textboxLength > 500"
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error">
            <span class="col-12 p-0">Tên checklist không quá 500 kí tự!</span>
          </small>
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12 px-0"
          v-if="
            (v$.checklist_name.$invalid && submitted) ||
            v$.checklist_name.$pending.$response
          "
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error">
            <span class="col-12 p-0">{{
              v$.checklist_name.required.$message
                .replace("Value", "Tên checklist")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="field col-12 md:col-12 format-center">
          <div class="col-3 text-left p-0">Mô tả</div>
          <InputText
            v-model="checkList.description"
            spellcheck="false"
            class="col-9 ip36 px-2"
            autocomplete="offnewInput"
          />
        </div>
        <div
          style="display: flex; align-items: center"
          class="col-12 field md:col-12"
        >
          <div class="col-3 text-left p-0">STT</div>
          <div class="col-9 p-0">
            <InputNumber
              v-model="checkList.is_order"
              class="w-full ip36 p-0"
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveData(!v$.$invalid)"
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerTask"
    v-model:visible="openTaskDialog"
    :style="{ width: '50vw' }"
    :closable="true"
    style="z-index: 10000"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 format-center">
          <label class="col-3 text-left p-0"
            >Tên công việc <span class="redsao">(*)</span></label
          >
          <Textarea
            id="textbox"
            v-model="Task.task_name"
            spellcheck="false"
            class="w-full"
            col="5"
            rows="2"
            :autoResize="true"
            :class="{
              'p-invalid': v1$.task_name.$invalid && submittedTask,
            }"
          />
        </div>

        <div
          style="display: flex"
          class="field col-12 md:col-12 px-0"
          v-if="
            (v1$.task_name.$invalid && submittedTask) ||
            v1$.task_name.$pending.$response
          "
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error">
            <span class="col-12 p-0">{{
              v1$.task_name.required.$message
                .replace("Value", "Tên checklist")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>

        <div
          class="field col-12 md:col-12 format-center"
          style="display: flex; align-items: center"
        >
          <div class="col-3 text-left p-0">Trọng số</div>
          <Dropdown
            :filter="true"
            v-model="Task.weight"
            :options="props.weight"
            optionLabel="weight_name"
            optionValue="weight"
            class="col-9 ip36 p-0"
            emptyMessage="Chưa có trọng số! Vui lòng thiết lập trọng số trước!"
          >
            <template #option="slotProps">
              <div class="country-item flex">
                <div class="pt-1">{{ slotProps.option.weight_name }}</div>
              </div>
            </template>
          </Dropdown>
        </div>
        <div class="field col-12 md:col-12 format-center flex">
          <div class="col-3 text-left format-left p-0">Ưu tiên</div>
          <div class="col-1 format-left p-0">
            <Checkbox
              :binary="true"
              v-model="Task.is_prioritize"
            />
          </div>
          <div class="col-2 text-left justify-content-center">Hoàn thành</div>
          <div class="col-1 format-left">
            <Checkbox
              :binary="true"
              v-model="Task.is_check"
            />
          </div>
          <div
            class="col-2 text-left justify-content-center align-items-center"
          >
            <label v-if="Task.is_check">Ngày hoàn thành</label>
          </div>
          <div
            class="col-3 pr-0"
            style="display: flex; padding: 0px; align-items: center"
          >
            <Calendar
              v-if="Task.is_check"
              :manualInput="true"
              :showIcon="true"
              class="w-full ip36 title-lable p-0 m-0"
              style="margin-top: 5px; padding: 0px"
              id="time1"
              autocomplete="on"
              :modelValue="new Date()"
              v-model="Task.close_date"
            />
          </div>
        </div>
        <div
          class="field col-12 md:col-12 format-center"
          style="display: flex; align-items: center"
        >
          <div class="col-3 text-left p-0 justify-content-center">
            Ngày bắt đầu
          </div>
          <div
            class="col-3"
            style="display: flex; padding: 0px; align-items: center"
          >
            <Calendar
              :manualInput="true"
              :showIcon="true"
              class="w-full ip36 title-lable"
              style="margin-top: 5px; padding: 0px"
              id="time1"
              autocomplete="on"
              v-model="Task.start_date"
              :modelValue="moment(new Date()).format('DD/MM/YYYY')"
            />
          </div>

          <div class="col-3">Hạn xử lý</div>
          <div
            class="col-3 pr-0"
            style="display: flex; padding: 0px; align-items: center"
          >
            <Calendar
              :manualInput="true"
              :showIcon="true"
              class="w-full ip36 title-lable p-0 m-0"
              style="margin-top: 5px; padding: 0px"
              id="time1"
              autocomplete="on"
              v-model="Task.end_date"
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="(openTaskDialog = false), (editTask = false)"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveTask(!v1$.$invalid)"
      />
    </template>
  </Dialog>
</template>
<style lang="scss" scoped>
::v-deep(.p-rowgroup-header) {
  td {
    width: 100% !important;
  }
  .p-row-toggler {
    vertical-align: middle;
    margin-right: 1rem;
    margin-left: 1rem;
  }
}
.bgVar {
  background-color: var(--bgHeader);
}
::v-deep(.p-datatable) {
  &.p-datatable-header {
    background: #f8f9fa;
    color: #495057;
    font-weight: 600;
  }
}
.TextColor {
  color: var(--text);
}
.checklist-hover:hover {
  background-color: #dbf4fd !important;
  cursor: pointer;
}
.activate1 {
  background-color: #2196f3;
  color: #fff !important;
}
.activate2 {
  color: #fff;
  font-style: oblique;
  font-weight: 100;
}
.checked-task {
  color: #6dd230;
  text-decoration: line-through;
  font-weight: bold;
}
.expired-task {
  color: #f00000;
}
</style>
