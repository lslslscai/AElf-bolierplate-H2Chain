using AElf.Contracts.MultiToken;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CreditTransferContract
{
    /// <summary>
    /// The C# implementation of the contract defined in credit_transfer_contract.proto that is located in the "protobuf"
    /// folder.
    /// Notice that it inherits from the protobuf generated code. 
    /// </summary>
    public class CreditTransferContract : CreditTransferContractContainer.CreditTransferContractBase
    {
        /// <summary>
        /// The implementation of the Initialize method. By using it, system can load necessary system contract and
        /// some needed parameters
        /// </summary>
        public override Empty Initialize(Empty input)
        {
            if (State.Initialized.Value)
            {
                return new Empty();
            }
            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            //接入系统合约（暂时没有）
            
            State.Initialized.Value = true;
            //第一个加入系统（启动区块链）的成为链管理者，他掌握着系统中全部的钱

            State.adminAddress.Value = Context.Sender;
            return new Empty();
        }
        
        /// <summary>
        /// Used to create SRT. Sender provides the SRT's StudentID, and system will create a SRT by it. 
        /// </summary>
        /// <param name="input">StudentID of new SRT</param>
        public override Empty SRT_Create(StringValue input)
        {
            //合成新SRT
            SRT newSRT = new SRT
            {
                StudentID = input.Value,
                Rating = 0,
                State = 0
            };
            
            //检查合法性
            Return_Solve(
                SRT_Validate(newSRT, CreditTransferContractConstants.CREATION_MODE),
                CreditTransferContractConstants.SRT_MODE
            );
            
            //记录
            State.SRT_Base[new StringValue {Value = input.Value}] = newSRT;
            
            return new Empty();
        }
        
        /// <summary>
        /// Used to modify SRT. Sender provides the new value of SRT (sender should provide all message,
        /// no matter how many changes have been made), and system will overwrite the old SRT by the inputted value.
        /// </summary>
        /// <param name="input">new SRT</param>
        public override Empty SRT_Adjust(SRT input)
        {
            //检查新值合法性和非空
            Return_Solve(
                SRT_Validate(input, CreditTransferContractConstants.MODIFY_MODE),
                CreditTransferContractConstants.SRT_MODE
            );
            
            //利用新值的StudentID查找旧值，并检查旧值合法性
            Return_Solve(
                SRT_Validate(
                    State.SRT_Base[new StringValue{Value = input.StudentID}],
                    CreditTransferContractConstants.INVOKE_MODE
                ),
                CreditTransferContractConstants.SRT_MODE
            );
            
            //到此，能够找到旧值与新值对应，且新旧值均合法
            //修改值
            State.SRT_Base[new StringValue {Value = input.StudentID}] = input;
            
            return new Empty();
        }
        
        /// <summary>
        /// Used to create CourseInfo. Sender provides the CourseInfo, and system will upload it.  
        /// </summary>
        /// <param name="input">CourseInfo for upload</param>
        public override Empty Course_Create(CourseInfo input)
        {
            //检查非空
            if(input == null) Return_Solve(2, CreditTransferContractConstants.CREATION_MODE);
            
            //检查合法性
            Return_Solve(
                Course_Validate(input, CreditTransferContractConstants.CREATION_MODE),
                CreditTransferContractConstants.COURSE_MODE
            );
            
            //记录
            State.CourseInfo_Base[new StringValue {Value = input.CourseID}] = input;
            
            return new Empty();
        }
        
        /// <summary>
        /// Used to modify CourseInfo. Sender provides the new value of CourseInfo (sender should provide all message,
        /// no matter how many changes have been made), and system will overwrite the old CourseInfo by the inputted
        /// value.
        /// </summary>
        /// <param name="input">new SRT</param>
        public override Empty Course_Adjust(CourseInfo input)
        {
            //检查新值合法性和非空
            Return_Solve(
                Course_Validate(input, CreditTransferContractConstants.MODIFY_MODE),
                CreditTransferContractConstants.COURSE_MODE
            );
            
            //利用新值的StudentID查找旧值，并检查旧值合法性
            Return_Solve(
                Course_Validate(
                    State.CourseInfo_Base[new StringValue{Value = input.CourseID}],
                    CreditTransferContractConstants.INVOKE_MODE
                ),
                CreditTransferContractConstants.COURSE_MODE
            );
            
            //到此，能够找到旧值与新值对应，且新旧值均合法
            //修改值
            State.CourseInfo_Base[new StringValue {Value = input.CourseID}] = input;
            
            return new Empty();
        }

        /// <summary>
        /// Used to register School. Sender provides the School, and system will upload it. It will only be invoked
        /// when a school registers in the system. 
        /// </summary>
        /// <param name="input">School intend to register</param>
        public override Empty School_Register(School input)
        {
            //检查输入合法性
            Return_Solve(
                School_Validate(input,CreditTransferContractConstants.CREATION_MODE),
                CreditTransferContractConstants.SCHOOL_MODE
            );
            
            //输入
            State.School_Base[new StringValue{Value = input.SchoolID}] = input;
            
            return new Empty();
        }
        
        /// <summary>
        /// Used to modify School. Sender provides the new value of School (sender should provide all message,
        /// no matter how many changes have been made), and system will overwrite the old School by the inputted
        /// value.
        /// </summary>
        /// <param name="input">new SRT</param>
        public override Empty School_Adjust(School input)
        {
            //检查新值合法性和非空
            Return_Solve(
                School_Validate(input, CreditTransferContractConstants.MODIFY_MODE),
                CreditTransferContractConstants.SCHOOL_MODE
            );
            
            //利用新值的StudentID查找旧值，并检查旧值合法性
            Return_Solve(
                School_Validate(
                    State.School_Base[new StringValue{Value = input.SchoolID}],
                    CreditTransferContractConstants.SCHOOL_MODE
                ),
                CreditTransferContractConstants.INVOKE_MODE
            );
            
            //到此，能够找到旧值与新值对应，且新旧值均合法
            //修改值
            State.School_Base[new StringValue{Value = input.SchoolID}] = input;
            
            return new Empty();
        }

        /// <summary>
        /// (core function) Used to select Course.
        /// Sender provides parameters, and system will upload it.  
        /// </summary>
        /// <param name="input">parameters needed, including student that want course, course to be selected,
        /// protocol and note about the courseRecord.</param>
        public override Empty SR_Select(SRUploadInput input)
        {
            //创建CourseRecord信息
            CourseRecord newRecord = new CourseRecord
            {
                CourseID = input.CourseID,
                StudentID = input.StudentID,
                Protocol = input.Protocol,
                State = false,
                GPA = 0,
                Score = 0,
                Note = input.Note
            };
            
            //检查选课信息合法性（包括课程、学生和协议的合法性）
            Return_Solve(
                SR_Validate(
                    newRecord,  
                    State.CourseInfo_Base[new StringValue{Value = input.CourseID}],
                    State.SRT_Base[new StringValue{Value = input.StudentID}],
                    CreditTransferContractConstants.CREATION_MODE,
                    input.Protocol
                ),CreditTransferContractConstants.SR_MODE
            );

            //记录
            State.CourseRecoed_Base[new StringValue
            {
                Value = input.CourseID + input.StudentID
            }] = newRecord;
            
            
            return new Empty();
        }
        
        /// <summary>
        /// (core function) Used to drop Course.
        /// Sender provides parameters, and system will upload it.  
        /// </summary>
        /// <param name="input">parameters needed, including studentID and courseID to specify the courseRecord.</param>
        public override Empty SR_Drop(SRDropInput input)
        {
            CourseRecord courseRecord = State.CourseRecoed_Base[new StringValue
            {
                Value = input.CourseID + input.StudentID
            }];
            //检查合法性
            Return_Solve(
                SR_Validate(
                    courseRecord, 
                    State.CourseInfo_Base[new StringValue{Value = input.CourseID}],
                    State.SRT_Base[new StringValue{Value = input.StudentID}],
                    CreditTransferContractConstants.INVOKE_MODE
                ),CreditTransferContractConstants.SR_MODE
            );
            
            //删除
            State.CourseRecoed_Base.Remove(new StringValue
            {
                Value = input.CourseID + input.StudentID
            });
            
            return new Empty();
        }
        
        /// <summary>
        /// (core function) Used to adjust Course. Includes 
        /// Sender provides parameters, and system will upload it.  
        /// </summary>
        /// <param name="input">parameters needed, including studentID and courseID to specify the courseRecord,
        /// and contents (state, score, GPA) to adjust
        /// protocol and note about the courseRecord.</param>
        public override Empty SR_Adjust(SRModifyInput input)
        {
            //提取旧值
            CourseRecord courseRecord = State.CourseRecoed_Base[new StringValue
            {
                Value = input.CourseID + input.StudentID
            }];
            
            //检查旧值合法性
            Return_Solve(
                SR_Validate(
                    courseRecord, 
                    State.CourseInfo_Base[new StringValue{Value = input.CourseID}],
                    State.SRT_Base[new StringValue{Value = input.StudentID}],
                    CreditTransferContractConstants.INVOKE_MODE
                ),CreditTransferContractConstants.SR_MODE
            );
            
            //根据input创建新值
            CourseRecord newRecord = new CourseRecord
            {
                CourseID = courseRecord.CourseID,
                StudentID = courseRecord.StudentID,
                Protocol = courseRecord.Protocol,
                State = input.State,
                GPA = input.GPA,
                Score = input.Score,
                Note = courseRecord.Note
            };
            
            //检查新值合法性
            Return_Solve(
                SR_Validate(
                    newRecord,  
                    State.CourseInfo_Base[new StringValue{Value = input.CourseID}],
                    State.SRT_Base[new StringValue{Value = input.StudentID}],
                    CreditTransferContractConstants.MODIFY_MODE,
                    courseRecord.Protocol
                ),CreditTransferContractConstants.SR_MODE
            );
            
            //修改
            State.CourseRecoed_Base[new StringValue
            {
                Value = input.CourseID + input.StudentID
            }] = newRecord;
            var rate = input.GPA * State.CourseInfo_Base[new StringValue
            {
                Value = input.CourseID
            }].Credit * CreditTransferContractConstants.INITIAL_RATING  / 400;
            State.SRT_Base[new StringValue {Value = input.StudentID}].Rating += rate;
            State.School_Base[new StringValue {Value = input.StudentID.Substring(0,5)}].Rating += rate;
            if (State.adminAddress.Value != State.School_Base[new StringValue
                {
                    Value = input.StudentID.Substring(0, 5)
                }].SchoolAddress)
            {
                State.TokenContract.TransferFrom.Send(new TransferFromInput
                {
                    Amount = long.Parse(rate.ToString()),
                    Memo = "transfer",
                    Symbol = Context.Variables.NativeSymbol,
                    From = State.adminAddress.Value,
                    To = State.School_Base[new StringValue {Value = input.StudentID.Substring(0,5)}].SchoolAddress
                });
            }
            return new Empty();
        }

        public override Empty Teacher_Register(Teacher input)
        {
            Return_Solve(
                Teacher_Validate(input, CreditTransferContractConstants.CREATION_MODE),
                CreditTransferContractConstants.TEACHER_MODE
            );

            State.Teacher_Base[input.TeacherAddress] = new StringValue{Value = input.TeacherID};
            
            return new Empty();
        }
        
        public override Empty Teacher_Adjust(Teacher input)
        {
            //检查新值
            Return_Solve(
                Teacher_Validate(input, CreditTransferContractConstants.MODIFY_MODE),
                CreditTransferContractConstants.TEACHER_MODE
            );
            
            //先验非空
            if(State.Teacher_Base[input.TeacherAddress] == null)
                Return_Solve(4, CreditTransferContractConstants.TEACHER_MODE);
            
            //构建旧值
            Teacher teacher = new Teacher
            {
                TeacherAddress = input.TeacherAddress,
                TeacherID = State.Teacher_Base[input.TeacherAddress].Value
            };
            
            //检查旧值
            Return_Solve(
                Teacher_Validate(teacher, CreditTransferContractConstants.INVOKE_MODE),
                CreditTransferContractConstants.TEACHER_MODE
            );
            
            //覆盖
            State.Teacher_Base[input.TeacherAddress] = new StringValue{Value = input.TeacherID};
            
            return new Empty();
        }

        public override School get_School(StringValue input)
        {
            School school = State.School_Base[input];
            Return_Solve(
                School_Validate(school,CreditTransferContractConstants.READ_MODE),
                CreditTransferContractConstants.SCHOOL_MODE
            );
            return school;
        }
        public override Address get_AdminAddress(Empty input)
        {
            return State.adminAddress.Value;
        }
        public override SRT get_SRT(StringValue input)
        {
            SRT student = State.SRT_Base[input];
            Return_Solve(
                SRT_Validate(
                    student,
                    CreditTransferContractConstants.READ_MODE
                ),
                CreditTransferContractConstants.SRT_MODE
            );
            return student;
        }
        
        public override CourseRecord get_CourseRecord(StringValue input)
        {
            CourseRecord SR = State.CourseRecoed_Base[input];
            if(SR == null) Return_Solve(4, CreditTransferContractConstants.SR_MODE);
            
            Return_Solve(
                SR_Validate(
                    SR,
                    State.CourseInfo_Base[new StringValue{Value = SR.CourseID}],
                    State.SRT_Base[new StringValue{Value = SR.StudentID}],
                    CreditTransferContractConstants.READ_MODE,
                    SR.Protocol
                ),
                CreditTransferContractConstants.SR_MODE
            );
            return SR;
        }
        
        public override CourseInfo get_CourseInfo(StringValue input)
        {
            CourseInfo courseInfo = State.CourseInfo_Base[input];
            Return_Solve(
                Course_Validate(
                    courseInfo,
                    CreditTransferContractConstants.READ_MODE
                ),
                CreditTransferContractConstants.COURSE_MODE
            );
            return courseInfo;
        }
        
        public override StringValue get_Teacher(Address input)
        {
            if(State.Teacher_Base[input] == null)
                Return_Solve(4, CreditTransferContractConstants.TEACHER_MODE);
            Teacher teacher = new Teacher
            {
                TeacherAddress = input,
                TeacherID = State.Teacher_Base[input].Value
            };
            Return_Solve(
                Teacher_Validate(
                    teacher,
                    CreditTransferContractConstants.READ_MODE
                ),
                CreditTransferContractConstants.TEACHER_MODE
            );
            return new StringValue{Value = teacher.TeacherID};
        }
        /// <summary>
        /// Used to solve return value from XXX_Validation. 
        /// </summary>
        /// <param name="input">return value to solve</param>
        /// <param name="mode">show where the return value is.</param>
        private void Return_Solve(int input, int mode)
        {
            string prefix = "";//根据mode生成前缀
            switch (mode)
            {
                case 0: prefix = "SRT";
                    break;
                case 1: prefix = "Course";
                    break;
                case 2: prefix = "School";
                    break;
                case 3: prefix = "Protocol";
                    break;
                case 4: prefix = "CourseRecord";
                    break;
                case 5: prefix = "Teacher";
                    break;
            }
            //0系列
            Assert(input != 1, prefix + " : sender identification failed!");
            Assert(input != 2, prefix + " : input invalid!");
            Assert(input != 3, prefix + " : intend to change data that can't change!");
            Assert(input != 4, prefix + " : content not found!");
            Assert(input != 5, prefix + " : content have already existed!");
            //1系列
            Assert(input != 101, prefix + " : system error!");
            Assert(input != 102, prefix + " : saved data incorrect!");
        }
        
        /// <summary>
        /// Used to Validate SRT. It checks studentID, Context.sender, state 
        /// </summary>
        /// <param name="input">SRT to check</param>
        /// <param name="mode">show where the SRT is checked.</param>
        /// <returns>an Error Code</returns>
        private int SRT_Validate(SRT input, int mode)
        {
            //先检查input是否为空
            if (input == null) 
                return (mode == 1 || mode == 3) ? 4 : 2;//如果是创建模式返回输入错误，否则返回未找到
            
            //检查studentID格式与内容
            if (input.StudentID.Length != 15)
                return (mode == 1 || mode == 3) ? 102 : 2;//输入编号格式不对，返回输入错误；否则（提取数据有问题），返回系统数据错误
            
            //提取学校
            string schoolID = input.StudentID.Substring(0, 5);
            School school = State.School_Base[new StringValue {Value = schoolID}];
            
            //检查发起者
            if (mode != 3)//如果不为只读模式，才需要检查发起者
            {
                if (school == null) return 4;//学校不存在，返回未找到
                if (school.SchoolAddress != Context.Sender)//若不是学校账户发起的，需要检查是不是教师
                {
                    if (State.Teacher_Base[Context.Sender] == null)
                        return 1;//试图修改数据的教师尚未注册，且不是学校账号进行修改，返回身份错误
                    string teacherID = State.Teacher_Base[Context.Sender].Value;
                    if (teacherID.Substring(0,5) != schoolID) 
                        return 1;//教师试图增改数据，但他并不属于目标学校，返回发起者身份错误
                }
            }
            
            

            //检查state
            if (mode == 1 && (input.State == 1 || input.State == 2)) return 3;//引用了不可修改状态
            if (input.State > 2)//非法状态（创建模式下任何非零状态，引用模式下非0，1，2状态）
                return (mode == 1 || mode == 3) ? 102 : 2;//状态输入不对，返回输入错误；否则（提取数据有问题），返回系统数据错误
            
            if (mode == 0 && State.SRT_Base[new StringValue { Value = input.StudentID }] != null)
                return 5;//学生信息已经存在，不可重复添加
            return 0;
        }
        
        /// <summary>
        /// Used to Validate CourseInfo. It checks courseID, Context.sender and contents.
        /// </summary>
        /// <param name="input">CourseInfo to check</param>
        /// <param name="mode">show where the CourseInfo is checked.</param>
        /// <returns>an Error Code</returns>
        private int Course_Validate(CourseInfo input, int mode)
        {
            //先检查input是否为空
            if (input == null) 
                return mode == 1 || mode == 3 ? 4 : 2;//如果是创建模式返回输入错误，否则返回未找到
            
            //提取学校和学生的ID
            string schoolID = input.CourseID.Substring(0, 5);
            School school = State.School_Base[new StringValue {Value = schoolID}];
            
            //检查输入数据格式与内容
            if (input.CourseID.Length != 12//ID长度不对
                || input.CourseType > 3)//类型不对
                return (mode == 1 || mode == 3) ? 102 : 2;//输入编号格式不对，返回输入错误；否则（提取数据有问题），返回系统数据错误
            
            //检查发起者
            if (mode != 3)
            {
                if (school == null) return 2;//学校不存在，返回输入错误
                if (school.SchoolAddress != Context.Sender 
                    && Context.Sender != State.adminAddress.Value)//若不是本学校账户发起，且不是超级管理员，需要检查是不是教师
                {
                    if (State.Teacher_Base[Context.Sender] == null)
                        return 1;//试图修改数据的教师尚未注册，且不是学校账号进行修改，返回身份错误
                    string teacherID = State.Teacher_Base[Context.Sender].Value;
                    if (teacherID.Substring(0,5) != schoolID) 
                        return 1;//教师试图增改数据，但他并不属于目标学校，返回发起者身份错误
                }
            }
            
            
            if (mode == 0 && State.CourseInfo_Base[new StringValue { Value = input.CourseID }] != null)
                return 5;//课程信息已经存在，不可重复添加
            
            return 0;
        }
        
        /// <summary>
        /// Used to Validate School. It checks courseID and Context.sender 
        /// </summary>
        /// <param name="input">School to check</param>
        /// <param name="mode">show where the School is checked.</param>
        /// <returns>an Error Code</returns>
        private int School_Validate(School input, int mode)
        {
            //先检查input是否为空
            if (input == null) 
                return (mode == 1 || mode == 3) ? 4 : 2;//如果是创建模式返回输入错误，否则返回未找到
            //经过测试，创建模式的错误无法进入，故山区
            
            //检查schoolID格式与内容
            if (input.SchoolID.Length != 5)
                return (mode == 1 || mode == 3) ? 102 : 2;//输入编号格式不对，返回输入错误；否则（提取数据有问题），返回系统数据错误

            //检查发起者
            if (mode != 3 && Context.Sender != State.adminAddress.Value)
                return 1;//试图增改学校信息，但发起者并非超级管理者，返回发起者身份错误
            
            //检查是否有重复数据
            if (mode == 0 && State.School_Base[new StringValue { Value = input.SchoolID }] != null)
                return 5;//学校信息已经存在，不可重复添加
            
            return 0;
        }
        
        /// <summary>
        /// Used to Validate Protocol. Because protocol will only be check when creating CourseRecord, parameter "mode"
        /// isn't needed. 
        /// </summary>
        /// <param name="input">Protocol to check</param>
        /// <returns>an Error Code</returns>
        private int Protocol_Validate(Protocol input)
        {
            //先检查input是否为空
            if (input == null) return 2;//协议为空，返回输入错误
            if (input.StartDate.Length != 10 
                || input.TestDate.Length != 10 
                || input.ProjectDate.Length != 10 )//日期格式有误，返回输入错误
                return 2;
            if (!date_Validation(input.StartDate, input.TestDate) ||
                !date_Validation(input.StartDate, input.ProjectDate))
                return 2;//日期有逻辑错误，返回输入错误
            if (input.RateOfProject + input.RateOfTest >= 100) return 2;//成绩分配不合理，返回输入错误
            return 0;
        }

        private bool date_Validation(string start, string end)
        {
            int startYear = int.Parse(start.Split('-')[0]);
            int startMonth = int.Parse(start.Split('-')[1]);
            int startDay = int.Parse(start.Split('-')[2]);
            
            int endYear = int.Parse(end.Split('-')[0]);
            int endMonth = int.Parse(end.Split('-')[1]);
            int endDay = int.Parse(end.Split('-')[2]);

            if (!date_check(start) || !date_check(end)) return false;//检查时间格式
            
            if (startYear > endYear//开始于2021，结束于2020（不可能）
                || (startYear == endYear && startMonth > endMonth)//开始于2021.2，结束于2021.1（不可能）
                || (startYear == endYear && startMonth == endMonth && startDay > endDay))//开始于2021.2.5，结束于2021.2.4（不可能）
                return false;
            return true;
        }
        
        private bool date_check(string start)
        {
            int year = int.Parse(start.Split('-')[0]);
            int month = int.Parse(start.Split('-')[1]);
            int day = int.Parse(start.Split('-')[2]);
            switch (month)
            {
                case 1: return day <= 31 && day >= 1;
                case 2: 
                    if(year % 4 != 0 || year % 400 != 0) return day <= 28 && day >= 1;
                    else return day <= 29 && day >= 1;
                case 3: return day <= 31 && day >= 1;
                case 4: return day <= 30 && day >= 1;
                case 5: return day <= 31 && day >= 1;
                case 6: return day <= 30 && day >= 1;
                case 7: return day <= 31 && day >= 1;
                case 8: return day <= 31 && day >= 1;
                case 9: return day <= 30 && day >= 1;
                case 10: return day <= 31 && day >= 1;
                case 11: return day <= 30 && day >= 1;
                case 12: return day <= 31 && day >= 1;
                default: return false;
            }
        }
        
        /// <summary>
        /// Used to Validate Teacher.
        /// </summary>
        /// <param name="input">Teacher to check</param>
        /// <returns>an Error Code</returns>
        private int Teacher_Validate(Teacher input, int mode)
        {
            //先检查input是否为空
            if (input == null) 
                return (mode == 1 || mode == 3) ? 4 : 2;//如果是创建模式返回输入错误，否则返回未找到

            if (input.TeacherID.Length != 11)//ID长度不对
                return (mode == 1 || mode == 3) ? 102 : 2;//输入编号格式不对，返回输入错误；否则（提取数据有问题），返回系统数据错误
            
            //提取所属学校信息
            string schoolID = input.TeacherID.Substring(0, 5);
            School school = State.School_Base[new StringValue {Value = schoolID}];
            //检查发起者
            if (mode != 3)//如果不为只读模式，才需要检查
            {
                if (school == null) return 2;//学校不存在，返回输入错误
                if (school.SchoolAddress != Context.Sender) 
                    return 1;//试图增改教师信息，但发起者并非学校系统账号，返回发起者身份错误
            }
            
            
            if (mode == 0 && State.Teacher_Base[input.TeacherAddress] != null)
                return 5;//用户已经存在，不可重复添加
            return 0;
        }
        
        /// <summary>
        /// Used to Validate CourseRecord. It checks course, student, protocol and courseRecord's score, GPA and state.
        /// </summary>
        /// <param name="courseRecord">courseRecord to check</param>
        /// <param name="course">course to check</param>
        /// <param name="student">student to check</param>
        /// <param name="mode">show where the CourseInfo is checked.</param>
        /// <param name="protocol">(optional) protocol to check (only when creating CourseRecord).</param>
        /// <returns>an Error Code</returns>
        private int SR_Validate(CourseRecord courseRecord, 
            CourseInfo course, SRT student, int mode, Protocol protocol = null)
        {
            int SRMode = mode == 0 ? 1 : mode;//创建模式下按照引用模式检查course，其他模式不变
            
            //检查选课信息非空
            if(courseRecord == null)
                return (mode == 1 || mode == 3) ? 4 : 2;//如果是创建模式返回输入错误，否则返回未找到
            
            //任何情况下按引用模式检查学生
            int SRT_val_ret = SRT_Validate(student,CreditTransferContractConstants.READ_MODE);
            if (SRT_val_ret != 0) return SRT_val_ret;
                
            //检查课程
            int course_val_ret = Course_Validate(course, SRMode);
            if (course_val_ret != 0) return course_val_ret;
            
            //引用模式下需要额外检查的合法性
            if (mode == 1)
            {
                if (courseRecord.State) return 3;//已经结课的选课记录不可修改或者删除，触发对应报错
            }
            
            //创建模式下需要额外检查的合法性
            if (mode == 0)
            {
                //只在创建时会涉及协议
                int proto_val_ret = Protocol_Validate(protocol);
                if (proto_val_ret != 0) return proto_val_ret;
                
                if(course.IsValid == false) return 2;//试图选已经不再开设的课程，返回输入错误

                if (State.CourseRecoed_Base[new StringValue {
                        Value = course.CourseID + student.StudentID
                    }] != null)
                    return 5;//课程信息已经存在，不可重复添加
            }
            
            //额外的数据合法性检查
            if (courseRecord.Score > 100_00)
                return (mode == 1 || mode == 3) ? 102 : 2;//分数非法，若是创建模式则返回输入错误；否则（提取数据有问题）返回系统数据错误
            if (courseRecord.GPA > 4_00)
                return (mode == 1 || mode == 3) ? 102 : 2;//绩点非法，若是创建模式则返回输入错误；否则（提取数据有问题）返回系统数据错误

            return 0;
        }
    }
}