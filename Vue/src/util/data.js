import { isObject, capitalizeFirstLetter } from "./function.js";
//Khởi tạo hàm
Date.prototype.addDays = function (days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
}
export const getDataTemp = () => {
    let today = new Date();
    today.setHours(0);
    let dts =
        [
            {
                "Tên công ty": "Công ty cổ phần phần mềm Phương Đông",
                "Logo công ty": '<img width="64" height="64" alt="" style="position: absolute;object-fit: cover;border-radius: 50%;" src="http://vtc.soe.vn/favicon.ico"/>',
                "Số": "QĐ.2208",
                "Giới tính": "Nam",
                "Sinh ngày": "10",
                "Tháng sinh": "12",
                "Năm sinh": "1988",
                "Nơi sinh": "Trạm y tế xã Đặng Xá, Gia Lâm, Hà Nội",
                "Ảnh": '<img width="129" height="160" alt="" style="object-fit: cover;margin-top:-0.82pt; margin-left:14.63pt; -aw-left-pos:15.75pt; -aw-rel-hpos:column; -aw-rel-vpos:paragraph; -aw-top-pos:0.3pt; -aw-wrap-type:none; position:absolute" src="https://apiv2.soe.vn//Portals/Users/44311433_1860466920669366_6024507152440229888_21013107585291.jpg"/>',
                "Điện thoại": "0987729288",
                "Dân tộc": "Kinh",
                "Tôn giáo": "Không",
                "Địa điểm": "Hà Nội",
                "Trình độ văn hoá": "Đại học",
                "Khen thưởng": "Tấm gương người tốt việc tốt",
                "Sở trường": "Công nghệ",
                "Ngày": (today.getDay() < 10 ? '0' : '') + today.getDay(),
                "Tháng": (today.getMonth() + 1 < 10 ? '0' : '') + (today.getMonth() + 1),
                "Năm": today.getFullYear(),
                "Họ và Tên": "Hoàng Đức Công",
                "Ngày sinh": "10/12/1988",
                "CMND": "0010 8803 2053",
                "Ngày cấp": "26/03/2019",
                "Nơi cấp": "Công an thành phố Hà Nội",
                "Ngày vào đoàn": "26/03/2003",
                "Địa điểm đoàn": "Trường tiểu học Đặng Xá",
                "Ngày vào đảng": "26/03/2018",
                "Địa điểm đảng": "Đại học BK Hà Nội",
                "Hộ khẩu TT": "Số 18/16/61 , Đường Ỷ Lan, Đặng Xá, Gia Lâm, Hà Nội",
                "Chỗ ở hiện tại": "Số 18/16/61 , Đường Ỷ Lan, Đặng Xá, Gia Lâm, Hà Nội",
                "Chức vụ": "Giám đốc kỹ thuật",
                "Quyền và nghĩa vụ": "Chịu trách nhiệm về các sản phẩm phát triển của công ty",
                "Chữ ký giám đốc": "<img height=96 src='https://cdn.fast.vn/tmp/20201020155613-9.PNG'/>",
                "Xác nhận": "Thông tin kê khai đúng với hồ sơ",
                "Tên cha": "Hoàng Văn Bá",
                "Năm sinh cha": "1957",
                "Nghề nghiệp cha": "Thương binh",
                "Cơ quan cha": "Hà Nội",
                "Chỗ ở cha": "Số 18/16/61 , Đường Ỷ Lan, Đặng Xá, Gia Lâm, Hà Nội",
                "Tên mẹ": "Nguyễn Thị Phượng",
                "Năm sinh mẹ": "1957",
                "Nghề nghiệp mẹ": "Giáo viên",
                "Cơ quan mẹ": "Hà Nội",
                "Chỗ ở mẹ": "Số 18/16/61 , Đường Ỷ Lan, Đặng Xá, Gia Lâm, Hà Nội",
                "Anh chị em": [
                    { "STT": 3, "Họ tên anh em": "Hoàng Thị Lan Anh", "Năm sinh anh em": "1983", "Nghề nghiệp anh em": "Điều dưỡng", "Cơ quan anh em": "Bệnh viện Xanh Pôn Hà Nội" },
                    { "STT": 4, "Họ tên anh em": "Hoàng Thị Minh Tú", "Năm sinh anh em": "1986", "Nghề nghiệp anh em": "Nhân viên Văn Phòng", "Cơ quan anh em": "Hàng không Quốc gia Malaysia" }
                ],
                "Đào tạo": [
                    { "Thời gian": "2006-2011", "Trường": "Đại học Bách Khoa Hà Nội", "Ngành học": "Cơ tin", "Hình thức": "Chính Quy", "Văn bằng": "Đại học" },
                    { "Thời gian": "2010-2012", "Trường": "FPT Aptech", "Ngành học": "CNTT", "Hình thức": "Liên thông", "Văn bằng": "Kỹ sư" },
                ],
                "Công tác": [
                    { "Thời gian": "2010-2011", "Đơn vị": "Công ty cổ phần ITNT", "Chức vụ": "Nhân viên" },
                    { "Thời gian": "2012-2014", "Đơn vị": "Công ty cổ phần phần mềm Phương Đông", "Chức vụ": "Nhân viên" },
                    { "Thời gian": "2014-2016", "Đơn vị": "Công ty cổ phần phần mềm Phương Đông", "Chức vụ": "Trưởng phòng kỹ thuật" },
                    { "Thời gian": "2016-2023", "Đơn vị": "Công ty cổ phần phần mềm Phương Đông", "Chức vụ": "Giám đốc kỹ thuật" },
                ]
            },
            {
                "Tên công ty": "Công ty cổ phần phần mềm Phương Đông",
                "Số": "02032023",
                "Địa điểm": "Hà Nội",
                "Ngày": (today.getDay() < 10 ? '0' : '') + today.getDay(),
                "Tháng": (today.getMonth() < 10 ? '0' : '') + today.getMonth(),
                "Năm": today.getFullYear(),
                "Họ và Tên": "Nguyễn Thành Chung",
                "Giới tính": "Nam",
                "Sinh ngày": "01",
                "Tháng sinh": "09",
                "Năm sinh": "1998",
                "Nơi sinh": "Trạm y tế Cẩm Phả, Quảng Ninh",
                "Ảnh": '<img width="129" height="160" alt="" style="object-fit: cover;margin-top:-0.82pt; margin-left:14.63pt; -aw-left-pos:15.75pt; -aw-rel-hpos:column; -aw-rel-vpos:paragraph; -aw-top-pos:0.3pt; -aw-wrap-type:none; position:absolute" src="https://sfile.soe.vn//Portals/CBA146D4A77C43259CC8231885703B78/NhanSu/Thumb/%E1%BA%A3nh%20s%E1%BA%AFp%20up%20av21110210285375.jpg"/>',
                "Ngày sinh": "01/09/1998",
                "CMND": "00109923456",
                "Ngày cấp": "20/02/2020",
                "Nơi cấp": "Công an tỉnh Quảng Ninh",
                "Dân tộc": "Kinh",
                "Tôn giáo": "Không",
                "Hộ khẩu TT": "Cẩm Phả, Quảng Ninh",
                "Đào tạo": [
                    { "Thời gian": "2018-2022", "Trường": "Học viện kỹ thuật quân sự", "Ngành học": "CNTT", "Hình thức": "Chính Quy", "Văn bằng": "Đại học" },
                ],
                "Công tác": [
                    { "Thời gian": "2019-2021", "Đơn vị": "Công ty cổ phần phần mềm Phương Đông", "Chức vụ": "Nhân viên" },
                    { "Thời gian": "2021-2023", "Đơn vị": "Công ty cổ phần phần mềm Phương Đông", "Chức vụ": "Trưởng phòng kỹ thuật" },
                ],
                "Chỗ ở hiện tại": "Đình Thôn, Mỹ Đình, Từ Liêm, Hà Nội",
                "Chức vụ": "Trưởng phòng kỹ thuật",
                "Quyền và nghĩa vụ": "Chịu trách nhiệm về các sản phẩm phát triển của công ty",
                "Chữ ký giám đốc": "<img height=96 src='https://tindep.com/wp-content/uploads/2019/04/mau-chu-ky-ten-huy-dep-nhat-1.png'/>"
            }
        ]
    let dts1 = [
        {
            "Họ và Tên": "Nguyễn Văn Tuyến",
            "captren": "BỘ QUỐC PHÒNG",
            "donvi": "BẢO HIỂM XÃ HỘI",
            "so": "00102023",
            "ngaycong": 25,
            "nghibu": "0",
            "nghikotinhphep": 0,
            "nghihungluong": 1,
            "nghiphep": 1,
            "mucluong": 15000000,
            "luongcoban": 4000000,
            "luonghieuqua": 5000000,
            "luongthemgio": 1000000,
            "congluong": 1000000,
            "truluong": 100000,
            "nghihuongluong": 0,
            "phucapdt": 300000,
            "phucapan": 200000,
            "congtacphi": 0,
            "tongluong": 15000000,
            "thuclinh": 15000000,
            "luongtrachnhiem": 500000,
            "baohiem": 200000,
            "thue": 1000000,
            "ngay": today.getDate(),
            "thang": (today.getMonth() + 1 < 10 ? '0' : '') + (today.getMonth() + 1).toString(),
            "nam": today.getFullYear(),
            "tungay": "13/03",
            "denngay": "20/03/2023",
            "dienthoai": "0987729288",
            "dongchi": "Nguyễn Khánh Toàn",
            "chucvu": "Trưởng phòng",
            "dongchitruc": "Nguyễn Thành Chung",
            "phong": "Phát triển sản phẩm",
            "lichs": [
                { "stt": "01", "hoten": "Hoàng Đức Công", "capbac": "Viên chức, P Giám đốc", "thoigian": '13/03/2023', "thu": "Thứ hai", "trucchihuy": "Phạm Thanh Hải" },
                { "stt": "02", "hoten": "Phạm Thanh Hải", "capbac": "Viên chức, Giám đốc", "thoigian": '14/03/2023', "thu": "Thứ ba", "trucchihuy": "Nguyễn Thị Dương" },
                { "stt": "03", "hoten": "Nguyễn Thành Chung", "capbac": "Viên chức, Trưởng phòng", "thoigian": '15/03/2023', "thu": "Thứ tư", "trucchihuy": "Phạm Thanh Hải" },
                { "stt": "04", "hoten": "Nguyễn Thị Hương", "capbac": "Viên chức, Nhân viên", "thoigian": '16/03/2023', "thu": "Thứ năm", "trucchihuy": "Hoàng Đức Công" },
                { "stt": "05", "hoten": "Mai Lê Thái Phiên", "capbac": "Viên chức, Nhân viên", "thoigian": '17/03/2023', "thu": "Thứ sáu", "trucchihuy": "Phạm Thanh Hải" },
                { "stt": "06", "hoten": "Lưu Thị Hân Hạnh", "capbac": "Viên chức, Nhân viên", "thoigian": '18/03/2023', "thu": "Thứ bảy", "trucchihuy": "Nguyễn Thị Dương" },
                { "stt": "07", "hoten": "Nguyễn Văn Tráng", "capbac": "Viên chức, Nhân viên", "thoigian": '19/03/2023', "thu": "Chủ nhật", "trucchihuy": "Phạm Thanh Hải" },
            ],
            "users": { "name1": "Phạm Thanh Hải", "name2": "Hoàng Đức Công", "name3": "Nguyễn Thị Dương" },
            "giamdocs": [
                {
                    "thu": "Thứ 2<br/>13/03",
                    "content1": `
                        - S: 08:00, Họp giao ban (ĐĐ: <i>Phòng giao ban</i>)<br/>
                        - C: 14:00, Làm việc tại cơ quan (ĐĐ: <i>Phòng làm việc</i>)
                        `,
                    "content2": `
                        - S: 08:00, Họp tại đối tác (ĐĐ: <i>Phòng họp VTC</i>)<br/>
                        - C: 13:00, Làm việc tại cơ quan (ĐĐ: <i>Phòng kỹ thuật</i>)
                        `,
                    "content3": `
                        - S: 08:00, Làm việc tại cơ quan (ĐĐ: <i>Phòng CSKH</i>)<br/>
                        - C: 14:00, Làm việc với Bic(ĐĐ: <i>Phòng họp Bic</i>)
                        `
                },
                {
                    "thu": "Thứ 3<br/>14/03",
                    "content1": `
                        - S: 08:00, Họp giao ban (ĐĐ: <i>Phòng giao ban</i>)<br/>
                        - C: 14:00, Làm việc tại cơ quan (ĐĐ: <i>Phòng làm việc</i>)
                        `,
                    "content2": `
                        - S: 08:00, Họp tại đối tác (ĐĐ: <i>Phòng họp VTC</i>)<br/>
                        - C: 13:00, Làm việc tại cơ quan (ĐĐ: <i>Phòng kỹ thuật</i>)
                        `,
                    "content3": `
                        - S: 08:00, Làm việc tại cơ quan (ĐĐ: <i>Phòng CSKH</i>)<br/>
                        - C: 14:00, Làm việc với Bic(ĐĐ: <i>Phòng họp Bic</i>)
                        `
                },
                {
                    "thu": "Thứ 4<br/>15/03",
                    "content1": `
                        - S: 08:00, Họp giao ban (ĐĐ: <i>Phòng giao ban</i>)<br/>
                        - C: 14:00, Làm việc tại cơ quan (ĐĐ: <i>Phòng làm việc</i>)
                        `,
                    "content2": `
                        - S: 08:00, Họp tại đối tác (ĐĐ: <i>Phòng họp VTC</i>)<br/>
                        - C: 13:00, Làm việc tại cơ quan (ĐĐ: <i>Phòng kỹ thuật</i>)
                        `,
                    "content3": `
                        - S: 08:00, Làm việc tại cơ quan (ĐĐ: <i>Phòng CSKH</i>)<br/>
                        - C: 14:00, Làm việc với Bic(ĐĐ: <i>Phòng họp Bic</i>)
                        `
                },
                {
                    "thu": "Thứ 5<br/>16/03",
                    "content1": `
                        Làm việc tại cơ quan
                        `,
                    "content2": `
                        Làm việc tại cơ quan
                        `,
                    "content3": `
                        Làm việc tại cơ quan
                        `
                },
                {
                    "thu": "Thứ 6<br/>17/03",
                    "content1": `
                        Làm việc tại cơ quan
                        `,
                    "content2": `
                        Làm việc tại cơ quan
                        `,
                    "content3": `
                        Làm việc tại cơ quan
                        `
                },
                {
                    "thu": "Thứ 7<br/>18/03",
                    "content1": `
                        Nghỉ
                        `,
                    "content2": `
                        Nghỉ
                        `,
                    "content3": `
                        Nghỉ
                        `
                },
                {
                    "thu": "CN<br/>19/03",
                    "content1": `
                        Nghỉ
                        `,
                    "content2": `
                        Nghỉ
                        `,
                    "content3": `
                        Nghỉ
                        `
                }
            ],
            "nguoiky": "Đại tá Trần Minh Thắng",
            "chuky": "<img height=96 src='https://tindep.com/wp-content/uploads/2019/04/mau-chu-ky-ten-huy-dep-nhat-1.png'/>",
        }
    ];
    dts.forEach(function (u, i) {
        if (i == 0) {
            u = Object.assign(u, dts1[i]);
        }
    })
    let st = 7;
    let nngay = (new Date()).addDays(6);
    let nngay1 = (new Date()).addDays(6);
    for (let i = 1; i <= 3; i++) {
        dts[0].giamdocs.forEach((element) => {
            nngay1 = nngay1.addDays(1);
            let ol = { ...element };
            ol.thu = ol.thu.split("<br/>")[0] + "<br/>" + (nngay1.getDate() < 10 ? '0' : '') + nngay1.getDate() + "/" + (nngay1.getMonth() < 10 ? '0' : '') + (nngay1.getMonth() + 1);
            dts[0].giamdocs.push(ol);
        });
        dts[0].lichs.forEach((element) => {
            nngay = nngay.addDays(1);
            st++;
            let ol = { ...element };
            ol.thoigian = (nngay.getDate() < 10 ? '0' : '') + nngay.getDate() + "/" + (nngay.getMonth() < 10 ? '0' : '') + (nngay.getMonth() + 1) + "/" + nngay.getFullYear();
            ol.stt = (st < 10 ? '0' : '') + st.toString();
            dts[0].lichs.push(ol);
        });
    }
    dts.forEach(u => {
        Object.keys(u).forEach(p => {
            if (isNaN(p) && u[p] && !Array.isArray(u[p]) && !isObject(u[p])) {
                u[p.toUpperCase()] = u[p].toString().toUpperCase();
                u[p.toLowerCase()] = u[p].toString().toLowerCase();
                u[capitalizeFirstLetter(p)] = capitalizeFirstLetter(u[p].toString(), true);
            }
        });
    });
    return dts;
};