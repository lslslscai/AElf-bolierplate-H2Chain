using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Cryptography.ECDSA;
using AElf.Kernel.Token;
using AElf.Types;
using AElf.Contracts.MultiToken;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AElf.Contracts.CreditTransferContract
{
    public class testConstants : CreditTransferContractTestBase
    {
        public static ECKeyPair AdminECKeyPair = SampleAccount.Accounts.First().KeyPair;
        public static Address AdminAddress = Address.FromPublicKey(AdminECKeyPair.PublicKey);
        
        public static ECKeyPair BobECKeyPair = SampleAccount.Accounts.ElementAt(1).KeyPair;
        public static Address BobSchoolAddress = Address.FromPublicKey(BobECKeyPair.PublicKey);

        public static ECKeyPair AliceECKeyPair = SampleAccount.Accounts.ElementAt(2).KeyPair;
        public static Address AliceSchoolAddress = Address.FromPublicKey(AliceECKeyPair.PublicKey);

        public static ECKeyPair bob = SampleAccount.Accounts.Reverse().ElementAt(1).KeyPair;
        public static Address bobAddress = Address.FromPublicKey(bob.PublicKey);

        public static ECKeyPair alice = SampleAccount.Accounts.Reverse().First().KeyPair;
        public static Address aliceAddress = Address.FromPublicKey(alice.PublicKey);

        public const string studentID = "100563018216282";
        public const string bobSchoolID = "10056";
        public const string bobTeacherID = "10056216001";
        public const string courseID = "100562160001";
        public const string aliceSchoolID = "10057";
        public const string aliceTeacherID = "10057216001";
    }
    
    public class SchoolTests : CreditTransferContractTestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SchoolTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Test for School_Register with good input
        /// </summary>
        [Fact]
        public async Task School_Register_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });

            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = testConstants.bobSchoolID
            });
            
            schoolRet.SchoolAddress.ShouldBe(testConstants.BobSchoolAddress);
        }

        /// <summary>
        /// Test for School_Register with input that have already existed
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task School_Register_Repeated_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            await stub.Initialize.SendAsync(new Empty());

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });

            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = testConstants.bobSchoolID
            });
            schoolRet.SchoolAddress.ShouldBe(testConstants.BobSchoolAddress);

            //自己进行重复创建，会因为重复添加数据而被阻止
            try
            {
                await stub.School_Register.SendAsync(new School
                {
                    SchoolID = testConstants.bobSchoolID,
                    SchoolAddress = testConstants.BobSchoolAddress,
                    Rating = 0
                });
            }
            catch(Exception e)
            {
                e.Message.ShouldContain("School : content have already existed!");
                return;
            }

            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for School_Adjust with good input
        /// </summary>
        [Fact]
        public async Task School_Adjust_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            await stub.Initialize.SendAsync(new Empty());

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });

            await stub.School_Adjust.SendAsync(new School{
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 100
            });
            
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = testConstants.bobSchoolID
            });
            
            schoolRet.Rating.ToString().ShouldBe("100");
        }
        
        /// <summary>
        /// Test for School_Register from wrong address (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task School_Register_Invalid_Address_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID, 
                SchoolAddress = testConstants.BobSchoolAddress, 
                Rating = 0
            });

            try
            {
                await bobSchoolStub.School_Register.SendAsync(new School
                {
                    SchoolID = testConstants.aliceSchoolID,
                    SchoolAddress = testConstants.AliceSchoolAddress,
                    Rating = 0
                });
                var schoolRet = await stub.get_School.CallAsync(new StringValue
                {
                    Value = testConstants.aliceSchoolID
                });
            
                schoolRet.SchoolAddress.ShouldBe(testConstants.AliceSchoolAddress);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("School : sender identification failed!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for School_Register with invalid SchoolID (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task School_Register_Invalid_ID_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            await stub.Initialize.SendAsync(new Empty());

            try
            {
                await stub.School_Register.SendAsync(new School
                {
                    SchoolID = "1005",
                    SchoolAddress = testConstants.BobSchoolAddress,
                    Rating = 0
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("School : input invalid!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for get_School with invalid search (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task School_Search_Not_Found_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            await stub.Initialize.SendAsync(new Empty());

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });
            //should failed and show assertionException before here
            try
            {
                var schoolRet = await stub.get_School.CallAsync(new StringValue
                {
                    Value = testConstants.aliceSchoolID
                });
                schoolRet.SchoolAddress.ShouldBe(testConstants.BobSchoolAddress);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("School : content not found!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for run get_School successfully
        /// </summary>
        [Fact]
        public async Task School_Search_Successful_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            await stub.Initialize.SendAsync(new Empty());

            var addr = await aliceStub.get_AdminAddress.CallAsync(new Empty());
            addr.ShouldBe(testConstants.AdminAddress);
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });
            //should failed and show assertionException before here
            var schoolRet = await aliceStub.get_School.CallAsync(new StringValue
            {
                Value = testConstants.bobSchoolID
            });
            schoolRet.SchoolAddress.ShouldBe(testConstants.BobSchoolAddress);
        }
    }

    public class TeacherTests : CreditTransferContractTestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TeacherTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        
        /// <summary>
        /// Test for Teacher_Register with good input
        /// </summary>
        [Fact]
        public async Task Teacher_Register_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            //bob所属学校创建bob教师信息，成功
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });

            await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherAddress = testConstants.bobAddress,
                TeacherID = testConstants.bobTeacherID
            });

            var teacherRet = await bobSchoolStub.get_Teacher.CallAsync(testConstants.bobAddress); 
            teacherRet.Value.ShouldBe(testConstants.bobTeacherID);
        }
        
        /// <summary>
        /// Test for Teacher_Register with input that have already existed
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Teacher_Register_Repeated_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });
            
            //bob所属学校创建bob教师信息，成功
            await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherAddress = testConstants.bobAddress,
                TeacherID = testConstants.bobTeacherID
            });

            var teacherRet = await bobSchoolStub.get_Teacher.CallAsync(testConstants.bobAddress); 
            teacherRet.Value.ShouldBe(testConstants.bobTeacherID);

            //自己进行重复创建，会因为重复添加数据而被阻止
            try
            {
                await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
                {
                    TeacherAddress = testConstants.bobAddress,
                    TeacherID = testConstants.bobTeacherID
                });
            }
            catch(Exception e)
            {
                e.Message.ShouldContain("Teacher : content have already existed!");
                return;
            }

            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for Teacher_Register with invalid TeacherID (Exception will be caught and its content will be tested)
        /// </summary>
        /// <remarks>can't detect wrong TeacherAddress! The correspondence between TeacherAddress and TeacherID should be
        /// guaranteed before sending transaction.</remarks>
        [Fact]
        public async Task Teacher_Register_Invalid_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });
            
            //按照错误ID创建，会因为格式不对而被阻止
            try
            {
                await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
                {
                    TeacherAddress = testConstants.bobAddress,
                    TeacherID = "1005621601"
                });
            }
            catch(Exception e)
            {
                e.Message.ShouldContain("Teacher : input invalid!");
                return;
            }
            
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for Teacher_Register from wrong address (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Teacher_Register_Invalid_Address_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.aliceSchoolID,
                SchoolAddress = testConstants.AliceSchoolAddress,
                Rating = 0
            });
            //bob所属学校创建bob教师信息，成功
            await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherAddress = testConstants.bobAddress,
                TeacherID = testConstants.bobTeacherID
            });

            var teacherRet = await bobSchoolStub.get_Teacher.CallAsync(testConstants.bobAddress); 
            teacherRet.Value.ShouldBe(testConstants.bobTeacherID);
            
            //超级管理员尝试创建老师，会因为增改不属于自己的数据而被阻止
            try
            {
                await stub.Teacher_Register.SendAsync(new Teacher
                {
                    TeacherAddress = testConstants.bobAddress,
                    TeacherID = testConstants.bobTeacherID
                });
            }
            catch(Exception e)
            {
                e.Message.ShouldContain("Teacher : sender identification failed!");
            }
            //由其他人进行创建，会因为增改不属于自己的数据而被阻止
            try
            {
                await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
                {
                    TeacherAddress = testConstants.aliceAddress,
                    TeacherID = testConstants.aliceTeacherID
                });
            }
            catch(Exception e)
            {
                e.Message.ShouldContain("Teacher : sender identification failed!");
                return;
            }
            
            throw new Exception("should not success!");
        }

        /// <summary>
        /// Test for get_Teacher with good input
        /// </summary>
        [Fact]
        public async Task Teacher_Search_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            
            //bob所属学校创建bob教师信息，成功
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });

            await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherAddress = testConstants.bobAddress,
                TeacherID = testConstants.bobTeacherID
            });

            var teacherRet = await bobSchoolStub.get_Teacher.CallAsync(testConstants.bobAddress); 
            teacherRet.Value.ShouldBe(testConstants.bobTeacherID);
        }
        
        /// <summary>
        /// Test for get_Teacher with invalid search (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Teacher_Search_Not_Found_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            //bob所属学校创建bob教师信息，成功
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });

            await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherAddress = testConstants.bobAddress,
                TeacherID = testConstants.bobTeacherID
            });
            
            try
            {
                var teacherRet = await bobSchoolStub.get_Teacher.CallAsync(testConstants.aliceAddress); 
            }
            catch(Exception e)
            {
                e.Message.ShouldContain("Teacher : content not found!");
                return;
            }
            
            throw new Exception("should not success!");
        }

        /// <summary>
        /// Test for Teacher_Adjust with good input
        /// </summary>
        [Fact]
        public async Task Teacher_Adjust_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            //bob所属学校创建bob教师信息，成功
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });

            await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherAddress = testConstants.bobAddress,
                TeacherID = testConstants.bobTeacherID
            });

            var teacherRet = await bobSchoolStub.get_Teacher.CallAsync(testConstants.bobAddress); 
            teacherRet.Value.ShouldBe(testConstants.bobTeacherID);
            
            await bobSchoolStub.Teacher_Adjust.SendAsync(new Teacher
            {
                TeacherAddress = testConstants.bobAddress,
                TeacherID = "10056216002"
            });
            
            var teacherRet1 = await bobSchoolStub.get_Teacher.CallAsync(testConstants.bobAddress); 
            teacherRet1.Value.ShouldBe("10056216002");
        }

        /// <summary>
        /// Test for Teacher_Adjust with invalid TeacherID or TeacherAddress that not exist
        ///  (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Teacher_Adjust_Invalid_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });
            
            await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherAddress = testConstants.bobAddress,
                TeacherID = testConstants.bobTeacherID
            });

            var teacherRet = await bobSchoolStub.get_Teacher.CallAsync(testConstants.bobAddress); 
            teacherRet.Value.ShouldBe(testConstants.bobTeacherID);
            
            //修改不存在的教师，会因为内容不存在而被阻止
            try
            {
                await bobSchoolStub.Teacher_Adjust.SendAsync(new Teacher
                {
                    TeacherAddress = testConstants.aliceAddress,
                    TeacherID = "10056216002"
                });
            }
            catch(Exception e)
            {
                e.Message.ShouldContain("Teacher : content not found!");
            }
            
            //按照错误ID修改，会因为格式不对而被阻止
            try
            {
                await bobSchoolStub.Teacher_Adjust.SendAsync(new Teacher
                {
                    TeacherAddress = testConstants.bobAddress,
                    TeacherID = "1005621601"
                });
            }
            catch(Exception e)
            {
                e.Message.ShouldContain("Teacher : input invalid!");
                return;
            }
            
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for Teacher_Adjust from wrong address (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Teacher_Adjust_Invalid_Address_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.bobSchoolID,
                SchoolAddress = testConstants.BobSchoolAddress,
                Rating = 0
            });
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = testConstants.aliceSchoolID,
                SchoolAddress = testConstants.AliceSchoolAddress,
                Rating = 0
            });
            
            //bob所属学校创建bob教师信息，成功
            await bobSchoolStub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherAddress = testConstants.bobAddress,
                TeacherID = testConstants.bobTeacherID
            });

            var teacherRet = await bobSchoolStub.get_Teacher.CallAsync(testConstants.bobAddress); 
            teacherRet.Value.ShouldBe(testConstants.bobTeacherID);
            
            //由其他人进行修改，会因为增改不属于自己的数据而被阻止
            try
            {
                await aliceSchoolStub.Teacher_Adjust.SendAsync(new Teacher
                {
                    TeacherAddress = testConstants.bobAddress,
                    TeacherID = "10056216002"
                });
            }
            catch(Exception e)
            {
                e.Message.ShouldContain("Teacher : sender identification failed!");
                return;
            }
            
            throw new Exception("should not success!");
        }
    }
    
    public class SRTTests : CreditTransferContractTestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        /// <summary>
        /// Method for creating School with parameters
        /// </summary>
        private async Task create_School(
            string schoolID, Address address,
            CreditTransferContractContainer.CreditTransferContractStub stub
            )
        {
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = schoolID,
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = schoolID
            });
            schoolRet.SchoolAddress.ShouldBe(address);
        }
        
        /// <summary>
        /// Method for creating Teacher with parameters
        /// </summary>
        private async Task create_Teacher(
            string teacherID, Address address,
            CreditTransferContractContainer.CreditTransferContractStub stub
        )
        {
            await stub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherID = teacherID,
                TeacherAddress = address,
            });
            var schoolRet = await stub.get_Teacher.CallAsync(address);
            schoolRet.Value.ShouldBe(teacherID);
        }
        
        /// <summary>
        /// Method for creating SRT with parameters
        /// </summary>
        private async Task create_SRT(
            string studentID,
            CreditTransferContractContainer.CreditTransferContractStub stub
        )
        {
            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = studentID
            });
            
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = studentID
            });
            studentRet.StudentID.ShouldBe(studentID);
            studentRet.State.ToString().ShouldBe("0");
        }

        /// <summary>
        /// Method for modify SRT with parameters
        /// </summary>
        private async Task adjust_SRT(
            SRT student,
            CreditTransferContractContainer.CreditTransferContractStub stub
        )
        {
            await stub.SRT_Adjust.SendAsync(student);
            
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = student.StudentID
            });
            studentRet.StudentID.ShouldBe(student.StudentID);
            studentRet.State.ToString().ShouldBe(student.State.ToString());
            studentRet.Rating.ToString().ShouldBe(student.Rating.ToString());
        }
        
        public SRTTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Test for SRT_Create with good input
        /// </summary>
        [Fact]
        public async Task SRT_Create_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);
            await create_SRT(testConstants.studentID, bobStub);
        }
        
        /// <summary>
        /// Test for SRT_Create with input that have already existed
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Create_Repeated_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);
            
            await create_SRT(testConstants.studentID, bobStub);
            try
            {
                await create_SRT(testConstants.studentID, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : content have already existed!");
                return;
            }

            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SRT_Adjust with good input
        /// </summary>
        [Fact]
        public async Task SRT_Adjust_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);
            await create_SRT(testConstants.studentID, bobStub);
            
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = testConstants.studentID
            });

            var newSRT = studentRet;
            newSRT.State = 1;
            newSRT.Rating = 100;
            await adjust_SRT(newSRT, bobStub);
        }
        
        /// <summary>
        /// Test for SRT_Create with invalid StudentID (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Create_Invalid_ID_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            //尝试创建ID格式错误的学生，会由于输入无效中止
            try
            {
                await create_SRT("10056301821628", bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : input invalid!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SRT_Create from wrong address (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Create_Invalid_Address_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            //尝试让school1的教师alice创建属于school的学生，会因为身份验证失败中止
            try
            {
                await create_SRT(testConstants.studentID, aliceStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : sender identification failed!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for get_SRT with invalid search (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Search_Not_Found_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_SRT(testConstants.studentID, bobStub);
            try
            {
                await aliceStub.get_SRT.CallAsync(new StringValue
                {
                    Value = "100563018216283"
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : content not found!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for run get_SRT successfully
        /// </summary>
        [Fact]
        public async Task SRT_Search_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_SRT(testConstants.studentID, bobStub);//创建SRT时已经完成检索了
            
        }
        
        /// <summary>
        /// Test for SRT_Adjust with invalid new state, school or SRT that not exist
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Adjust_Invalid_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_SRT(testConstants.studentID, bobStub);//创建SRT时已经完成检索了
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = testConstants.studentID
            });
            //studentRet.StudentID.ShouldBe(studentID);
            //尝试修改不存在的学生，会由于数据不存在中止
            try
            {
                var newSRT = studentRet.Clone();
                newSRT.State = 1; 
                newSRT.StudentID = "100563018216283";//不存在的学生
                await adjust_SRT(newSRT, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : content not found!");
            }
            
            //尝试修改所属学校不存在的学生，会由于输入无效被终止
            try
            {
                var newSRT1 = studentRet.Clone();
                newSRT1.State = 1; 
                newSRT1.StudentID = "100583018216282";//不存在的学校(如果输入10057的话会因为修改不属于本校的学生被阻止)
                await adjust_SRT(newSRT1, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : content not found!");
            }
            
            //尝试使用错误State修改数据，会由于输入无效中止
            try
            {
                var newSRT2 = studentRet.Clone();
                newSRT2.State = 3;//无效的State
                newSRT2.Rating = 100;
                await adjust_SRT(newSRT2, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : input invalid!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SRT_Adjust from wrong address (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Adjust_Invalid_Address_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_SRT(testConstants.studentID, bobStub);//创建SRT时已经完成检索了
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = testConstants.studentID
            });
            
            //尝试让school1的教师alice修改属于school的学生，会因为身份验证失败中止
            try
            {
                var newSRT = studentRet;
                newSRT.State = 1;
                await aliceStub.SRT_Adjust.SendAsync(newSRT);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : sender identification failed!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SRT_Adjust with accessing data that can't change
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Adjust_Blocked_Change_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_SRT(testConstants.studentID, bobStub);//创建SRT时已经完成检索了
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = testConstants.studentID
            });
            
            var newSRT = studentRet;
            newSRT.State = 1;
            await adjust_SRT(newSRT, bobStub);//修改结果验证已经在这里做了
            var studentRet1 = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = testConstants.studentID
            });
            
            //尝试修改已经不再开设的课程的信息（不可修改），会因为不可修改而终止
            try
            {
                var newSRT1 = studentRet1;
                newSRT1.Rating = 100;
                await adjust_SRT(newSRT1, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : intend to change data that can't change!");
                return;
            }
            throw new Exception("should not success!");
        }
    }
    
    public class CourseTests : CreditTransferContractTestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public CourseTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Method for creating School with parameters
        /// </summary>
        private async Task create_School(
            string schoolID, Address address,
            CreditTransferContractContainer.CreditTransferContractStub stub
        )
        {
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = schoolID,
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = schoolID
            });
            schoolRet.SchoolAddress.ShouldBe(address);
        }
        
        /// <summary>
        /// Method for creating Teacher with parameters
        /// </summary>
        private async Task create_Teacher(
            string teacherID, Address address,
            CreditTransferContractContainer.CreditTransferContractStub stub
        )
        {
            await stub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherID = teacherID,
                TeacherAddress = address,
            });
            var schoolRet = await stub.get_Teacher.CallAsync(address);
            schoolRet.Value.ShouldBe(teacherID);
        }
        
        /// <summary>
        /// Method for creating Course with parameters
        /// </summary>
        private async Task create_Course(
            string courseID,
            CreditTransferContractContainer.CreditTransferContractStub stub
        )
        {
            await stub.Course_Create.SendAsync(new CourseInfo
            {
                CourseID = courseID,
                IsCompulsory = true,
                CourseType = 0,
                IsValid = true,
                Credit = 4
            });
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = courseID
            });
            courseRet.CourseID.ShouldBe(courseID);
            courseRet.IsValid.ShouldBe(true);
        }
        
        /// <summary>
        /// Test for Course_Create with good input
        /// </summary>
        [Fact]
        public async Task Course_Create_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_Course(testConstants.courseID, bobStub);
        }
        
        /// <summary>
        /// Test for Course_Create with input that have already existed
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Create_Repeated_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_Course(testConstants.courseID, bobStub);
            try
            {
                await create_Course(testConstants.courseID, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : content have already existed");
                return;
            }

            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for Course_Adjust with good input
        /// </summary>
        [Fact]
        public async Task Course_Adjust_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_Course(testConstants.courseID, bobStub);
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = testConstants.courseID
            });
            courseRet.CourseID.ShouldBe(testConstants.courseID);
            courseRet.IsValid.ShouldBe(true);

            //改成选修课
            var newCourse = courseRet.Clone();
            newCourse.IsCompulsory = false;
            await bobStub.Course_Adjust.SendAsync(newCourse);
            var courseRet1 = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = testConstants.courseID
            });
            courseRet1.IsCompulsory.ShouldBe(false);
            
            //关闭课程
            var newCourse2 = courseRet1.Clone();
            newCourse2.IsValid = false;
            await bobStub.Course_Adjust.SendAsync(newCourse2);
            var courseRet2 = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = testConstants.courseID
            });
            courseRet2.IsValid.ShouldBe(false);
            courseRet2.IsCompulsory.ShouldBe(false);
        }
        
        /// <summary>
        /// Test for Course_Create with invalid CourseID (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Create_Invalid_ID_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_Course(testConstants.courseID, bobStub);

            try
            {
                await create_Course("10056216001", bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : input invalid!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for Course_Create with invalid CourseType (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Create_Invalid_Type_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            try
            {
                await bobStub.Course_Create.SendAsync(new CourseInfo
                {
                    CourseID = testConstants.courseID,
                    IsCompulsory = true,
                    CourseType = 4,
                    IsValid = true,
                    Credit = 4
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : input invalid!");
                return;
            }
            throw new Exception("should not success!");
        }

        /// <summary>
        /// Test for Course_Create from wrong address (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Create_Invalid_Address_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            //让alice创建课程（并不是alice所代表学校的课程）
            try
            {
                await create_Course(testConstants.courseID, aliceStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : sender identification failed!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for get_CourseInfo with invalid search (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Search_Not_Found_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_Course("100562160002", bobStub);
            try
            {
                await aliceStub.get_CourseInfo.CallAsync(new StringValue
                {
                    Value = testConstants.courseID
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : content not found!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for run get_CourseInfo successfully
        /// </summary>
        [Fact]
        public async Task Course_Search_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_Course(testConstants.courseID, bobStub);
            var courseRet = await aliceStub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = testConstants.courseID
            });
            courseRet.CourseID.ShouldBe(testConstants.courseID);
            courseRet.IsValid.ShouldBe(true);
        }
        
        /// <summary>
        /// Test for Course_Adjust with invalid CourseType or CourseID that isn't exist(Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Adjust_Invalid_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_Course(testConstants.courseID, bobStub);
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = testConstants.courseID
            });
            courseRet.CourseID.ShouldBe(testConstants.courseID);
            courseRet.IsValid.ShouldBe(true);
            try
            {
                var newCourse = courseRet.Clone();
                newCourse.CourseType = 4;
                await bobStub.Course_Adjust.SendAsync(newCourse);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : input invalid!");
            }
            try
            {
                var newCourse = courseRet.Clone();
                newCourse.CourseID = "100562160002";
                await stub.Course_Adjust.SendAsync(newCourse);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : content not found!");
            }
            try
            {
                var newCourse = courseRet.Clone();
                newCourse.CourseID = "100582160001";
                await stub.Course_Adjust.SendAsync(newCourse);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : input invalid!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for Course_Adjust from wrong address (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Adjust_Invalid_Address_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
                
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await create_Course(testConstants.courseID, bobStub);
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = testConstants.courseID
            });
            courseRet.CourseID.ShouldBe(testConstants.courseID);
            courseRet.IsValid.ShouldBe(true);

            try
            {
                var newCourse = courseRet;
                newCourse.IsCompulsory = false;
                await aliceStub.Course_Adjust.SendAsync(newCourse);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : sender identification failed!");
                return;
            }
            throw new Exception("should not success!");
        }
    }
    
    public class CourseRecordTests : CreditTransferContractTestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public CourseRecordTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Method for creating School with parameters
        /// </summary>
        private async Task create_School(
            string schoolID, Address address,
            CreditTransferContractContainer.CreditTransferContractStub stub
            )
        {
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = schoolID,
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = schoolID
            });
            schoolRet.SchoolAddress.ShouldBe(address);
        }
        
        /// <summary>
        /// Method for creating School with parameters
        /// </summary>
        private async Task create_Teacher(
            string teacherID, Address address,
            CreditTransferContractContainer.CreditTransferContractStub stub
        )
        {
            await stub.Teacher_Register.SendAsync(new Teacher
            {
                TeacherID = teacherID,
                TeacherAddress = address,
            });
            var schoolRet = await stub.get_Teacher.CallAsync(address);
            schoolRet.Value.ShouldBe(teacherID);
        }
        
        /// <summary>
        /// Method for creating SRT with parameters
        /// </summary>
        private async Task create_SRT(
            string studentID,
            CreditTransferContractContainer.CreditTransferContractStub stub
        )
        {
            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = studentID
            });
            
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = studentID
            });
            studentRet.StudentID.ShouldBe(studentID);
            studentRet.State.ToString().ShouldBe("0");
        }
        
        /// <summary>
        /// Method for creating Course with parameters
        /// </summary>
        private async Task create_Course(
            string courseID,
            CreditTransferContractContainer.CreditTransferContractStub stub
        )
        {
            await stub.Course_Create.SendAsync(new CourseInfo
            {
                CourseID = courseID,
                IsCompulsory = true,
                CourseType = 0,
                IsValid = true,
                Credit = 4
            });
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = courseID
            });
            courseRet.CourseID.ShouldBe(courseID);
            courseRet.IsValid.ShouldBe(true);
        }

        /// <summary>
        /// Method for selecting course with parameters to construct CourseRecord
        /// </summary>
        private async Task select_SR(
            string schoolID, string studentID, string courseID, string teacherID,
            Address schoolAddress, Address teacherAdress, Protocol SRProtocol,
            CreditTransferContractContainer.CreditTransferContractStub stub,
            CreditTransferContractContainer.CreditTransferContractStub schoolStub,
            CreditTransferContractContainer.CreditTransferContractStub teacherStub)
        {
            await create_School(schoolID, schoolAddress, stub);
            await create_Teacher(teacherID, teacherAdress, schoolStub);
            await create_SRT(studentID, teacherStub);
            await create_Course(courseID, teacherStub);
            
            await stub.SR_Select.SendAsync(new SRUploadInput
            {
                CourseID = courseID,
                StudentID = studentID,
                Protocol = SRProtocol,
                Note = "select lesson"
            });
        }
        
        /// <summary>
        /// Method for droping course with parameters to delete CourseRecord
        /// </summary>
        private async Task drop_SR(
            string studentID, string courseID,
            CreditTransferContractContainer.CreditTransferContractStub stub)
        {
            await stub.SR_Drop.SendAsync(new SRDropInput
            {
                CourseID = courseID,
                StudentID = studentID
            });
        }
        
        /// <summary>
        /// Method for adjusting course with parameters to modify CourseRecord
        /// </summary>
        private async Task adjust_SR(
            string studentID, string courseID, bool state, ulong GPA, ulong score,
            CreditTransferContractContainer.CreditTransferContractStub teacherStub)
        {
            await teacherStub.SR_Adjust.SendAsync(new SRModifyInput
            {
                CourseID = courseID,
                StudentID = studentID,
                State = state,
                GPA = GPA,
                Score = score
            });
        }
        
        /// <summary>
        /// Test for SR_Select with good input
        /// </summary>
        [Fact]
        public async Task SR_Select_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };
            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress, proto,
                stub,bobSchoolStub,bobStub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + testConstants.studentID
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
        }

        /// <summary>
        /// Test for SR_Select cross schools
        /// </summary>
        [Fact]
        public async Task SR_Select_Cross_School_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);
            
            await create_SRT("100573018216282", aliceStub);
            await create_Course(testConstants.courseID, bobStub);

            await stub.SR_Select.SendAsync(new SRUploadInput
            {
                CourseID = testConstants.courseID,
                StudentID = "100573018216282",
                Protocol = proto,
                Note = "select lesson"
            });

            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + "100573018216282"
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            
            try
            {
                //老师尝试修改外校的选课信息，会因为身份而失败（注意，不是因为已经存在数据才错误，身份检查在重复检查前）
                await aliceStub.SR_Select.SendAsync(new SRUploadInput
                {
                    CourseID = testConstants.courseID,
                    StudentID = "100573018216282",
                    Protocol = proto,
                    Note = "select lesson"
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : sender identification failed!");
                return;
            }
            throw new Exception("should not success!");
        }

        /// <summary>
        /// Test for SR_Select with good input
        /// </summary>
        [Fact]
        public async Task SR_Select_Repeated_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };
            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress,proto,
                stub,bobSchoolStub,bobStub);

            try
            {
                await stub.SR_Select.SendAsync(new SRUploadInput
                {
                    CourseID = testConstants.courseID,
                    StudentID = testConstants.studentID,
                    Protocol = proto,
                    Note = "select lesson"
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content have already existed!");
            }
            
        }
        
        /// <summary>
        /// Test for SR_Select with invalid StudentID or CourseID
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SR_Select_Invalid_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);
            
            await create_SRT(testConstants.studentID, bobStub);
            await create_Course(testConstants.courseID, bobStub);
            
            try
            {
                await stub.SR_Select.SendAsync(new SRUploadInput
                {
                    CourseID = testConstants.courseID,
                    StudentID = "100563018216283",
                    Protocol = proto,
                    Note = "select lesson"
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
            }

            try
            {
                await stub.SR_Select.SendAsync(new SRUploadInput
                {
                    CourseID = "100562160002",
                    StudentID = testConstants.studentID,
                    Protocol = proto,
                    Note = "select lesson"
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
                return;
            }
            
            throw new Exception("should not success!");
        } 
        
        /// <summary>
        /// Test for SR_Select from wrong address
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SR_Select_Invalid_Address_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);
            
            await create_SRT(testConstants.studentID, bobStub);
            await create_Course(testConstants.courseID, bobStub);

            try
            {
                await aliceStub.SR_Select.SendAsync(new SRUploadInput
                {
                    CourseID = testConstants.courseID,
                    StudentID = testConstants.studentID,
                    Protocol = proto,
                    Note = "select lesson"
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : sender identification failed!");
                return;
            }
            
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SR_Drop with good input
        /// </summary>
        [Fact]
        public async Task SR_Drop_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };
            
            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress, proto,
                stub,bobSchoolStub,bobStub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + testConstants.studentID
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");

            await drop_SR(testConstants.studentID, testConstants.courseID, stub);
            
            try
            {
                var SR_Result1 = await stub.get_CourseRecord.CallAsync(new StringValue
                {
                    Value = testConstants.courseID + testConstants.studentID
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
                return;
            }

            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SR_Adjust cross schools
        /// </summary>
        [Fact]
        public async Task SR_Drop_Cross_School_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);
            
            await create_SRT("100573018216282", aliceStub);
            await create_Course(testConstants.courseID, bobStub);

            await stub.SR_Select.SendAsync(new SRUploadInput
            {
                CourseID = testConstants.courseID,
                StudentID = "100573018216282",
                Protocol = proto,
                Note = "select lesson"
            });

            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + "100573018216282"
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            
            try
            {
                //老师尝试删除学生在外校的选课信息，失败
                await drop_SR("100573018216282", testConstants.courseID, aliceStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : sender identification failed!");
            }
            
            await drop_SR("100573018216282", testConstants.courseID, bobStub);
            try
            {
                var SR_Result1 = await stub.get_CourseRecord.CallAsync(new StringValue
                {
                    Value = testConstants.courseID + "100573018216282"
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
                return;
            }
            
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SR_Drop from wrong address
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SR_Drop_Invalid_Address_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };
            
            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress, proto,
                stub,bobSchoolStub,bobStub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + testConstants.studentID
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            
            try
            {
                await drop_SR(testConstants.studentID, testConstants.courseID, aliceStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : sender identification failed!");
                return;
            }

            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SR_Drop with invalid studentID or courseID
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SR_Drop_Invalid_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };
            
            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress, proto,
                stub,bobSchoolStub,bobStub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + testConstants.studentID
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            
            try
            {
                await drop_SR("100563018216283", testConstants.courseID, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
            }

            try
            {
                await drop_SR(testConstants.studentID, "100562160002", stub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SR_Adjust with good input
        /// </summary>
        [Fact]
        public async Task SR_Adjust_Success_Test()
        {
            var tokenStub =
                GetTester<TokenContractContainer.TokenContractStub>(
                    GetAddress(TokenSmartContractAddressNameProvider.StringName), testConstants.AdminECKeyPair);
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            var addr = await aliceSchoolStub.get_AdminAddress.CallAsync(new Empty());
            addr.ShouldBe(testConstants.AdminAddress);

            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress, proto,
                stub,bobSchoolStub,bobStub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + testConstants.studentID
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");

            var currBalance = await tokenStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = testConstants.AdminAddress,
                Symbol = "ELF"
            });

            await tokenStub.Approve.SendAsync(new ApproveInput
            {
                Spender = DAppContractAddress,
                Amount = currBalance.Balance,
                Symbol = "ELF"
            });

            await adjust_SR(testConstants.studentID, testConstants.courseID, true, 4_00, 100_00, bobStub);

            var SR_Result1 = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + testConstants.studentID
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue {Value = testConstants.bobSchoolID});
            schoolRet.Rating.ToString().ShouldBe("400000");
            SR_Result1.CourseID.ShouldBe(testConstants.courseID);
            SR_Result1.StudentID.ShouldBe(testConstants.studentID);
            SR_Result1.GPA.ToString().ShouldBe("400");
            SR_Result1.Score.ToString().ShouldBe("10000");

        }
        
        /// <summary>
        /// Test for SR_Adjust cross schools
        /// </summary>
        [Fact]
        public async Task SR_Adjust_Cross_School_Test()
        {
            var tokenStub =
                GetTester<TokenContractContainer.TokenContractStub>(
                    GetAddress(TokenSmartContractAddressNameProvider.StringName), testConstants.AdminECKeyPair);
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            await create_Teacher(testConstants.bobTeacherID, testConstants.bobAddress, bobSchoolStub);
            
            await create_SRT("100573018216282", aliceStub);
            await create_Course(testConstants.courseID, bobStub);

            await stub.SR_Select.SendAsync(new SRUploadInput
            {
                CourseID = testConstants.courseID,
                StudentID = "100573018216282",
                Protocol = proto,
                Note = "select lesson"
            });

            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + "100573018216282"
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            
            var currBalance = await tokenStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = testConstants.AdminAddress,
                Symbol = "ELF"
            });

            await tokenStub.Approve.SendAsync(new ApproveInput
            {
                Spender = DAppContractAddress,
                Amount = currBalance.Balance,
                Symbol = "ELF"
            });
            await adjust_SR("100573018216282", testConstants.courseID, true, 4_00, 100_00, bobStub);
            
            var SR_Result1 = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + "100573018216282"
            });
            
            SR_Result1.CourseID.ShouldBe(testConstants.courseID);
            SR_Result1.StudentID.ShouldBe("100573018216282");
            SR_Result1.GPA.ToString().ShouldBe("400");
            SR_Result1.Score.ToString().ShouldBe("10000");
            var schoolRet = await stub.get_School.CallAsync(new StringValue {Value = testConstants.aliceSchoolID});
            schoolRet.Rating.ToString().ShouldBe("400000");
            try
            {
                //老师尝试修改外校的选课信息，失败
                await adjust_SR("100573018216282", testConstants.courseID, true, 4_00, 100_00, aliceStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : sender identification failed!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SR_Adjust with invalid GPA ,score, StudentID or CourseID
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SR_Adjust_Invalid_Input_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress, proto,
                stub,bobSchoolStub,bobStub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + testConstants.studentID
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            try
            {
                await adjust_SR(testConstants.studentID, testConstants.courseID, true, 4_01, 100_00, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : input invalid!");
            }
            
            try
            {
                await adjust_SR("100563018216283", testConstants.courseID, true, 4_01, 100_00, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
            }
            
            try
            {
                await adjust_SR(testConstants.studentID, "100562160002", true, 4_01, 100_00, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
            }
            
            try
            {
                await adjust_SR(testConstants.studentID, testConstants.courseID, true, 4_00, 100_01, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : input invalid!");
                return;
            }
            throw new Exception("should not success!");
        } 
        
        /// <summary>
        /// Test for SR_Adjust from wrong address
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SR_Adjust_Invalid_Address_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress,proto,
                stub,bobSchoolStub,bobStub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + testConstants.studentID
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            try
            {
                await adjust_SR(testConstants.studentID, testConstants.courseID, true, 4_00, 100_00, aliceStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : sender identification failed!");
                return;
            }
            
            throw new Exception("should not success!");
        } 
        
        /// <summary>
        /// Test for SR_Adjust with accessing data that can't change
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SR_Adjust_Blocked_Change_Test()
        {
            var tokenStub =
                GetTester<TokenContractContainer.TokenContractStub>(
                    GetAddress(TokenSmartContractAddressNameProvider.StringName), testConstants.AdminECKeyPair);
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress,proto,
                stub,bobSchoolStub,bobStub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + testConstants.studentID
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            var currBalance = await tokenStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = testConstants.AdminAddress,
                Symbol = "ELF"
            });

            await tokenStub.Approve.SendAsync(new ApproveInput
            {
                Spender = DAppContractAddress,
                Amount = currBalance.Balance,
                Symbol = "ELF"
            });
            await adjust_SR(testConstants.studentID, testConstants.courseID, true, 4_00, 100_00, bobStub);
            
            try
            {
                await adjust_SR(testConstants.studentID, testConstants.courseID, true, 3_60, 89_00, bobStub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : intend to change data that can't change!");
                return;
            }
            throw new Exception("should not success!");
        } 
        
        /// <summary>
        /// Test for run get_CourseRecord successfully
        /// </summary>
        [Fact]
        public async Task SR_Search_Success_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);

            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress,proto,
                stub,bobSchoolStub,bobStub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = testConstants.courseID + testConstants.studentID
            });
            
            SR_Result.CourseID.ShouldBe(testConstants.courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
        }
        
        /// <summary>
        /// Test for get_CourseRecord with wrong CourseID or StudentID
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SR_Search_Not_Found_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var bobStub = GetCreditTransferContractStub(testConstants.bob);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);
            var aliceStub = GetCreditTransferContractStub(testConstants.alice);
            
            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());
            
            var proto = new Protocol
            {
                StartDate = "2022-03-01",
                ProjectDate = "2022-06-20",
                TestDate = "2022-06-20",
                RateOfProject = 60,
                RateOfTest = 30
            };

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_Teacher(testConstants.aliceTeacherID, testConstants.aliceAddress, aliceSchoolStub);
            
            await select_SR(testConstants.bobSchoolID, testConstants.studentID, 
                testConstants.courseID, testConstants.bobTeacherID,
                testConstants.BobSchoolAddress, testConstants.bobAddress,proto,
                stub,bobSchoolStub,bobStub);
            
            try
            {
                var SR_Result = await aliceStub.get_CourseRecord.CallAsync(new StringValue
                {
                    Value = testConstants.courseID + "100563018216283"
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
            }

            try
            {
                var SR_Result1 = await aliceStub.get_CourseRecord.CallAsync(new StringValue
                {
                    Value = "100562160002" + testConstants.studentID
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
                return;
            }
            
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for ELF token transfer between admin and schools
        /// </summary>
        [Fact]
        public async Task Token_Transfer_Test()
        {
            var stub = GetCreditTransferContractStub(testConstants.AdminECKeyPair);
            
            var bobSchoolStub = GetCreditTransferContractStub(testConstants.BobECKeyPair);
            var aliceSchoolStub = GetCreditTransferContractStub(testConstants.AliceECKeyPair);

            await stub.Initialize.SendAsync(new Empty());
            await bobSchoolStub.Initialize.SendAsync(new Empty());
            await aliceSchoolStub.Initialize.SendAsync(new Empty());

            await create_School(testConstants.aliceSchoolID, testConstants.AliceSchoolAddress, stub);
            await create_School(testConstants.bobSchoolID, testConstants.BobSchoolAddress, stub);
            
            var tokenStub =
                GetTester<TokenContractContainer.TokenContractStub>(
                    GetAddress(TokenSmartContractAddressNameProvider.StringName), testConstants.AdminECKeyPair);

            // Prepare awards.
            await tokenStub.Transfer.SendAsync(new TransferInput
            {
                To = DAppContractAddress,
                Symbol = "ELF",
                Amount = 100_00000000
            });

            var ret = await tokenStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = DAppContractAddress,
                Symbol = "ELF"
            });
            ret.Balance.ShouldBe(100_00000000);
        }
    }
    
}