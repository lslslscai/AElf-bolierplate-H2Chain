using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AElf.Contracts.CreditTransferContract
{
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
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });

            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            
            schoolRet.SchoolAddress.ShouldBe(address);
        }
        
        /// <summary>
        /// Test for School_Adjust with good input
        /// </summary>
        [Fact]
        public async Task School_Adjust_Success_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });

            await stub.School_Adjust.SendAsync(new School{
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 100
            });
            
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            
            schoolRet.Rating.ToString().ShouldBe("100");
        }
        
        /// <summary>
        /// Test for School_Register from wrong address (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task School_Register_Invalid_Address_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceAddress = Address.FromPublicKey(alice.PublicKey);
            var aliceStub = GetCreditTransferContractStub(alice);
            try
            {
                await aliceStub.School_Register.SendAsync(new School
                {
                    SchoolID = "10056",
                    SchoolAddress = address,
                    Rating = 0
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("School : sender identification failed!");
            }
            
            try
            {
                await stub.School_Register.SendAsync(new School
                {
                    SchoolID = "10056",
                    SchoolAddress = aliceAddress,
                    Rating = 0
                });
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            try
            {
                await stub.School_Register.SendAsync(new School
                {
                    SchoolID = "1005",
                    SchoolAddress = address,
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            //should failed and show assertionException before here
            try
            {
                var schoolRet = await stub.get_School.CallAsync(new StringValue
                {
                    Value = "10057"
                });
                schoolRet.SchoolAddress.ShouldBe(address);
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            //should failed and show assertionException before here
            var schoolRet = await aliceStub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);
        }
    }
    
    public class SRTTests : CreditTransferContractTestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

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
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = "100563018216282"
            });
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet.StudentID.ShouldBe("100563018216282");
            studentRet.State.ToString().ShouldBe("0");
        }
        
        /// <summary>
        /// Test for SRT_Adjust with good input
        /// </summary>
        [Fact]
        public async Task SRT_Adjust_Success_Test()
        {
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = "100563018216282"
            });
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet.StudentID.ShouldBe("100563018216282");
            studentRet.State.ToString().ShouldBe("0");

            var newSRT = studentRet;
            newSRT.State = 1;
            newSRT.Rating = 100;
            
            await stub.SRT_Adjust.SendAsync(newSRT);
            var studentRet1 = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet1.StudentID.ShouldBe("100563018216282");
            studentRet1.State.ToString().ShouldBe("1");
            studentRet1.Rating.ToString().ShouldBe("100");
        }
        
        /// <summary>
        /// Test for SRT_Create with invalid StudentID (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Create_Invalid_ID_Test()
        {
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);
            try
            {
                await stub.SRT_Create.SendAsync(new StringValue
                {
                    Value = "10056301821628"
                });
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
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);
            try
            {
                await aliceStub.SRT_Create.SendAsync(new StringValue
                {
                    Value = "100563018216282"
                });
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
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);
            
            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = "100563018216282"
            });
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet.StudentID.ShouldBe("100563018216282");
            studentRet.State.ToString().ShouldBe("0");
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
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);
            
            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = "100563018216282"
            });
            var studentRrt = await aliceStub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet.StudentID.ShouldBe("100563018216282");
            studentRet.State.ToString().ShouldBe("0");
        }
        
        /// <summary>
        /// Test for SRT_Adjust with invalid new state (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Adjust_New_Invalid_State_Test()
        {
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = "100563018216282"
            });
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet.StudentID.ShouldBe("100563018216282");
            studentRet.State.ToString().ShouldBe("0");

            try
            {
                var newSRT = studentRet;
                newSRT.State = 3;
                await stub.SRT_Adjust.SendAsync(newSRT);
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
        public async Task SRT_Adjust_Old_Invalid_Address_Test()
        {
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = "100563018216282"
            });
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet.StudentID.ShouldBe("100563018216282");
            studentRet.State.ToString().ShouldBe("0");

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
        public async Task SRT_Adjust_Old_Blocked_Change_Test()
        {
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = "100563018216282"
            });
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet.StudentID.ShouldBe("100563018216282");
            studentRet.State.ToString().ShouldBe("0");
            
            var newSRT = studentRet;
            newSRT.State = 1;
            await stub.SRT_Adjust.SendAsync(newSRT);
                
            var studentRet1 = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet1.State.ToString().ShouldBe("1");
            try
            {
                var newSRT1 = studentRet;
                newSRT1.Rating = 100;
                await stub.SRT_Adjust.SendAsync(newSRT1);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : intend to change data that can't change!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SRT_Adjust with changing data that not exists
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Adjust_Old_Nonexistent_SRT_Test()
        {
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = "100563018216282"
            });
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet.StudentID.ShouldBe("100563018216282");
            studentRet.State.ToString().ShouldBe("0");
            
            try
            {
                var newSRT = studentRet;
                newSRT.StudentID = "100563018216283";
                await stub.SRT_Adjust.SendAsync(newSRT);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : content not found!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        /// <summary>
        /// Test for SRT_Adjust with changing data whose school isn't exists
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SRT_Adjust_Old_Nonexistent_School_Test()
        {
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.SRT_Create.SendAsync(new StringValue
            {
                Value = "100563018216282"
            });
            var studentRet = await stub.get_SRT.CallAsync(new StringValue
            {
                Value = "100563018216282"
            });
            studentRet.StudentID.ShouldBe("100563018216282");
            studentRet.State.ToString().ShouldBe("0");
            
            try
            {
                var newSRT = studentRet;
                newSRT.StudentID = "100573018216282";
                await stub.SRT_Adjust.SendAsync(newSRT);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("SRT : input invalid!");
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
        /// Test for Course_Create with good input
        /// </summary>
        [Fact]
        public async Task Course_Create_Success_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.Course_Create.SendAsync(new CourseInfo
            {
                CourseID = "100562160001",
                IsCompulsory = true,
                CourseType = 0,
                IsValid = true
            });
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = "100562160001"
            });
            courseRet.CourseID.ShouldBe("100562160001");
            courseRet.IsValid.ShouldBe(true);
        }
        
        /// <summary>
        /// Test for Course_Adjust with good input
        /// </summary>
        [Fact]
        public async Task Course_Adjust_Success_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);
            
            //创建课程
            await stub.Course_Create.SendAsync(new CourseInfo
            {
                CourseID = "100562160001",
                IsCompulsory = true,
                CourseType = 0,
                IsValid = true
            });
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = "100562160001"
            });
            courseRet.CourseID.ShouldBe("100562160001");
            courseRet.IsValid.ShouldBe(true);

            //改成选修课
            var newCourse = courseRet;
            newCourse.IsCompulsory = false;
            await stub.Course_Adjust.SendAsync(newCourse);
            var courseRet1 = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = "100562160001"
            });
            courseRet1.IsCompulsory.ShouldBe(false);
            
            //关闭课程
            var newCourse2 = courseRet;
            newCourse2.IsValid = false;
            await stub.Course_Adjust.SendAsync(newCourse2);
            var courseRet2 = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = "100562160001"
            });
            courseRet2.IsValid.ShouldBe(false);
        }
        
        /// <summary>
        /// Test for Course_Create with invalid CourseID (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Create_Invalid_ID_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            try
            {
                await stub.Course_Create.SendAsync(new CourseInfo
                {
                    CourseID = "10056216001",
                    IsCompulsory = true,
                    CourseType = 0,
                    IsValid = true
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
        /// Test for Course_Create with invalid CourseType (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Create_Invalid_Type_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            try
            {
                await stub.Course_Create.SendAsync(new CourseInfo
                {
                    CourseID = "10056216001",
                    IsCompulsory = true,
                    CourseType = 4,
                    IsValid = true
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);
            try
            {
                //让alice创建课程（并不是alice所代表学校的课程）
                await aliceStub.Course_Create.SendAsync(new CourseInfo
                {
                    CourseID = "100562160001",
                    IsCompulsory = true,
                    CourseType = 0,
                    IsValid = true
                });
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
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.Course_Create.SendAsync(new CourseInfo
            {
                CourseID = "100562160002",
                IsCompulsory = true,
                CourseType = 0,
                IsValid = true
            });
            try
            {
                await aliceStub.get_CourseInfo.CallAsync(new StringValue
                {
                    Value = "100562160001"
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
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.Course_Create.SendAsync(new CourseInfo
            {
                CourseID = "100562160001",
                IsCompulsory = true,
                CourseType = 0,
                IsValid = true
            });
            var courseRet = await aliceStub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = "100562160001"
            });
            courseRet.CourseID.ShouldBe("100562160001");
            courseRet.IsValid.ShouldBe(true);
        }
        
        /// <summary>
        /// Test for Course_Adjust with invalid CourseType (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Adjust_New_Invalid_Type_Test()
        {
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.Course_Create.SendAsync(new CourseInfo
            {
                CourseID = "100562160001",
                IsCompulsory = true,
                CourseType = 0,
                IsValid = true
            });
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = "100562160001"
            });
            courseRet.CourseID.ShouldBe("100562160001");
            courseRet.IsValid.ShouldBe(true);
            try
            {
                var newCourse = courseRet;
                newCourse.CourseType = 4;
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
        public async Task Course_Adjust_Old_Invalid_Address_Test()
        {
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            
            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.Course_Create.SendAsync(new CourseInfo
            {
                CourseID = "100562160001",
                IsCompulsory = true,
                CourseType = 0,
                IsValid = true
            });
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = "100562160001"
            });
            courseRet.CourseID.ShouldBe("100562160001");
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
        
        
        /// <summary>
        /// Test for Course_Adjust with changing data that not exists
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Adjust_Old_Nonexistent_Course_Test()
        {
             
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.Course_Create.SendAsync(new CourseInfo
            {
                CourseID = "100562160001",
                IsCompulsory = true,
                CourseType = 0,
                IsValid = true
            });
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = "100562160001"
            });
            courseRet.CourseID.ShouldBe("100562160001");
            courseRet.IsValid.ShouldBe(true);
            
            try
            {
                var newCourse = courseRet;
                newCourse.CourseID = "100562160002";
                await stub.Course_Adjust.SendAsync(newCourse);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : content not found!");
                return;
            }
            throw new Exception("should not success!");
        }
        
        
        /// <summary>
        /// Test for Course_Adjust with changing data whose school isn't exists
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task Course_Adjust_Old_Nonexistent_School_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            await stub.School_Register.SendAsync(new School
            {
                SchoolID = "10056",
                SchoolAddress = address,
                Rating = 0
            });
            var schoolRet = await stub.get_School.CallAsync(new StringValue
            {
                Value = "10056"
            });
            schoolRet.SchoolAddress.ShouldBe(address);

            await stub.Course_Create.SendAsync(new CourseInfo
            {
                CourseID = "100562160001",
                IsCompulsory = true,
                CourseType = 0,
                IsValid = true
            });
            var courseRet = await stub.get_CourseInfo.CallAsync(new StringValue
            {
                Value = "100562160001"
            });
            courseRet.CourseID.ShouldBe("100562160001");
            courseRet.IsValid.ShouldBe(true);
            
            try
            {
                var newCourse = courseRet;
                newCourse.CourseID = "100572160001";
                await stub.Course_Adjust.SendAsync(newCourse);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Course : input invalid!");
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
                IsValid = true
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
            string schoolID, string studentID, string courseID, Address address,
            CreditTransferContractContainer.CreditTransferContractStub stub)
        {
            await create_School(schoolID, address, stub);
            await create_SRT(studentID, stub);
            await create_Course(courseID, stub);
            
            await stub.SR_Select.SendAsync(new SRUploadInput
            {
                CourseID = courseID,
                StudentID = studentID,
                Protocol = new Protocol
                {
                    ProtoID = courseID
                },
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
            CreditTransferContractContainer.CreditTransferContractStub stub)
        {
            await stub.SR_Adjust.SendAsync(new SRModifyInput
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            await select_SR(schoolID, studentID, courseID, address, stub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = courseID + studentID
            });
            
            SR_Result.CourseID.ShouldBe(courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
        }
        
        /// <summary>
        /// Test for SR_Select with invalid StudentID or CourseID
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SR_Select_Invalid_Input_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";
            
            await create_School(schoolID, address, stub);
            await create_SRT(studentID, stub);
            await create_Course(courseID, stub);
            
            try
            {
                await stub.SR_Select.SendAsync(new SRUploadInput
                {
                    CourseID = courseID,
                    StudentID = "100563018216283",
                    Protocol = new Protocol
                    {
                        ProtoID = courseID
                    },
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
                    StudentID = studentID,
                    Protocol = new Protocol
                    {
                        ProtoID = courseID
                    },
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            await create_School(schoolID, address, stub);
            await create_SRT(studentID, stub);
            await create_Course(courseID, stub);

            try
            {
                await aliceStub.SR_Select.SendAsync(new SRUploadInput
                {
                    CourseID = courseID,
                    StudentID = studentID,
                    Protocol = new Protocol
                    {
                        ProtoID = courseID
                    },
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            await select_SR(schoolID, studentID, courseID, address, stub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = courseID + studentID
            });
            
            SR_Result.CourseID.ShouldBe(courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");

            await drop_SR(studentID, courseID, stub);
            
            try
            {
                var SR_Result1 = await stub.get_CourseRecord.CallAsync(new StringValue
                {
                    Value = courseID + studentID
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            await select_SR(schoolID, studentID, courseID, address, stub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = courseID + studentID
            });
            
            SR_Result.CourseID.ShouldBe(courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            
            try
            {
                await drop_SR(studentID, courseID, aliceStub);
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);

            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            await select_SR(schoolID, studentID, courseID, address, stub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = courseID + studentID
            });
            
            SR_Result.CourseID.ShouldBe(courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            
            try
            {
                await drop_SR("100563018216283", courseID, stub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
            }

            try
            {
                await drop_SR(studentID, "100562160002", stub);
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            await select_SR(schoolID, studentID, courseID, address, stub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = courseID + studentID
            });
            
            SR_Result.CourseID.ShouldBe(courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");

            await adjust_SR(studentID, courseID, true, 4_00, 100_00, stub);
            
            var SR_Result1 = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = courseID + studentID
            });
            
            SR_Result1.CourseID.ShouldBe(courseID);
            SR_Result1.StudentID.ShouldBe(studentID);
            SR_Result1.GPA.ToString().ShouldBe("400");
            SR_Result1.Score.ToString().ShouldBe("10000");
        }
        
        /// <summary>
        /// Test for SR_Adjust with invalid GPA ,score, StudentID or CourseID
        /// (Exception will be caught and its content will be tested)
        /// </summary>
        [Fact]
        public async Task SR_Adjust_New_Invalid_Input_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            await select_SR(schoolID, studentID, courseID, address, stub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = courseID + studentID
            });
            
            SR_Result.CourseID.ShouldBe(courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            try
            {
                await adjust_SR(studentID, courseID, true, 4_01, 100_00, stub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : input invalid!");
            }
            
            try
            {
                await adjust_SR("100563018216283", courseID, true, 4_01, 100_00, stub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
            }
            
            try
            {
                await adjust_SR(studentID, "100562160002", true, 4_01, 100_00, stub);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
            }
            
            try
            {
                await adjust_SR(studentID, courseID, true, 4_00, 100_01, stub);
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
        public async Task SR_Adjust_Old_Invalid_Address_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            await select_SR(schoolID, studentID, courseID, address, stub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = courseID + studentID
            });
            
            SR_Result.CourseID.ShouldBe(courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            try
            {
                await adjust_SR(studentID, courseID, true, 4_00, 100_00, aliceStub);
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
        public async Task SR_Adjust_Old_Blocked_Change_Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            await select_SR(schoolID, studentID, courseID, address, stub);
            
            var SR_Result = await stub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = courseID + studentID
            });
            
            SR_Result.CourseID.ShouldBe(courseID);
            SR_Result.State.ShouldBeFalse();
            SR_Result.Note.ShouldBe("select lesson");
            
            await adjust_SR(studentID, courseID, true, 4_00, 100_00, stub);
            
            try
            {
                await adjust_SR(studentID, courseID, true, 3_60, 89_00, stub);
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);

            await select_SR(schoolID, studentID, courseID, address, stub);
            
            var SR_Result = await aliceStub.get_CourseRecord.CallAsync(new StringValue
            {
                Value = courseID + studentID
            });
            
            SR_Result.CourseID.ShouldBe(courseID);
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
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var address = Address.FromPublicKey(keyPair.PublicKey);
            var stub = GetCreditTransferContractStub(keyPair);
            
            var alice = SampleAccount.Accounts.Reverse().First().KeyPair;
            var aliceStub = GetCreditTransferContractStub(alice);
            
            var schoolID = "10056";
            var studentID = "100563018216282";
            var courseID = "100562160001";

            await select_SR(schoolID, studentID, courseID, address, stub);
            try
            {
                var SR_Result = await aliceStub.get_CourseRecord.CallAsync(new StringValue
                {
                    Value = courseID + "100563018216283"
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
                    Value = "100562160002" + studentID
                });
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("CourseRecord : content not found!");
                return;
            }
            
            throw new Exception("should not success!");
        }
    }
}