using Application.Common.Interfaces;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        protected ISender Mediator;

        public JwtService(ISender mediator)
        {
            Mediator = mediator;
        }

        public static string secret = "6f8907dd@df04@4556@9b46@9131300552e6f29aa5bf@f687@4ff8@9183@401b0852a1c9db297e9e@7282@4e3a@86af@7f53d2f01fa240c3cb99@9ce8@4352@a7d7@9f6ea595881469a32bcf@6228@489c@8766@26dc3d8b8837009b57c7@acc7@4f5c@8f4f@4f8269e724d490688a76@1099@4a49@9f52@ed3d54a7ee42740cc7dc@b6a0@4984@9dcc@09e106191c5b2ef2bca5@2e3c@43c3@b6be@a6082904bb00e6d0c14f@242f@4873@8e19@fd2678218d1890b984eb@cecd@4674@bff9@52bb2c400f3ef9de57b5@cb1c@48fd@ba04@d9505a50510a905c0fed@2710@47e6@96b0@a47efab9806babcdb69a@8386@4e71@a8d2@3d58e8fc6f5c56d42144@1d9f@4adf@b536@3b55b818dcfb378730b6@2210@458b@bdbf@f2b4133ad679ca94f635@b515@48d1@a090@8c6da37bcdc01efe62e8@7e81@4075@9742@1efe46e515295c09de25@dca8@4e0f@874c@549049271f685c3d32ed@58b3@42a9@9d1b@1ca6f48fbf38712699a7@b028@4e0d@ad19@893ab50d27587cbe4298@4cdc@4749@b48a@5edb110bfb18ff481ea8@9def@4f63@9b70@de1cd18d56f0704d3c03@4ba5@4085@b5c4@38548ddd4bcceaf49dad@d52b@48b0@889b@e1f116b2df5a8843d22b@d6cb@4ec3@b35a@e7ad294bb7f1135ef218@04b8@4c98@9706@d5d34b2821bf6d8d2e6f@d829@4688@8c3c@c70e7e97ee434d4dd3f5@b700@42a9@9e09@44b47387289f5aea636b@0111@43c1@9d87@da360eee283e591572ef@0d3f@449c@a134@575bef9cb5dc6fd2af5e@d8db@402d@9cd9@d32480a20ed5a50046f3@ea23@4e4f@8952@70b586ce6639862d8190@2c1b@4f8f@bf10@a88c82b3c62b0822c4cc@b2dc@4d2b@a929@45478432ca0dd4692cdf@7e0f@4c47@98a7@68e8f1ff93c5758d42b0@34ee@4e7c@a160@1dd5c204d121e4013161@5b71@410a@927f@8941938baa3bdda145d7@6e0b@4b35@8624@9264be9ff0f83d1c606b@3929@40c4@9de3@ddba25223c5d0957b42a@387f@4d9d@98a4@4391ed46fb855f97ad5b@e4e8@4a8d@8de3@28a2da3b9dc4c79bb291@eff5@4db7@9e90@268552ac23be0835f030@c71b@4e49@ad03@b6e6254a590a41846fb3@7d1a@4097@9a2e@8d8c38d495173a7d0e5f@cbe4@478c@894b@ca3f36f04b04709356a2@b7e7@4d6a@8f1c@11320efb317620364a83@2ff0@4c21@b144@38d16da4dfa7dd544cd3@ab21@4456@981e@f0d7ed511939eb47d546@9f44@4dcb@b64a@36a7f6cb07643b0f0e96@3d74@4b81@a5c3@3a22b2a4726a9460a3e2@60f3@4dfa@bdf1@444de24763d497ee98e7@9c12@43fc@a88a@ed840fedd9251b39cbfa@10ec@40c1@a384@97b4f492e36cfe96f78f@91f9@4349@a799@953f5cb43a6d556dec4b@f74e@48c9@a5b3@ce2852c22a2feccbdd54@aef6@427c@866a@202994433af937326400@b022@4441@aaa5@1f4ebbfc2e1f78f6aece@94d7@4036@97c2@1ba0f6eafbed9c45fd32@95b3@4969@8325@f8a3a877a69cacbbfe06@ff9f@4914@be06@741079b0bcceba87ce28@2d84@452b@a742@24d4cee89c734e580a2d@c17e@4a65@9f68@166df97e7cd20f61e6c3@25dd@433e@a435@33eb22e09e46a180a08d@f9e8@4c3b@a5da@55d50bbd73658b9585d1@ebab@4fe3@9a83@2010ac66141a3a6105db@e9cc@48bd@8666@3b4b617265ad3aa97174@3061@4dd1@92b5@50de1897046297e2fc05@d396@4c86@bb4b@27348a4db3c290234e72@69c7@4dbc@a43b@ac047e73eae2bbda01c1@746f@4be2@a7eb@22faf660ff4085c35883@d7e5@4234@a192@c23aa611f5ab4492be2b@dae1@46d7@ba95@541aec5d57b6db406b4f@460f@4f64@9d0a@b40e58942192b648b2f9@1f5a@49fd@a5cb@e57b16371f96065e1da8@eaaa@49d6@8160@09709ab5822dd02b8228@388e@4939@9630@13a1e0246591271b1a4f@c46d@4dae@a928@ed311302130f08d89dbf@150d@4e8d@84b9@1832c5ee1035ad12dce4@554f@4616@ac8e@4dd041ab0cb56fb451a5@ade1@468e@85f6@9d578d50a514956d8782@b6b6@496e@a0e8@844e305f85c74420a352@2d2c@4d7f@8571@6930055996d14c863ef1@d807@409a@86db@639854c9726cbbd3f6b6@8883@483e@9877@69e34e8263c4fe17ad0d@eee9@4ee9@b7a2@b7e9a80e49a5d95ab0e2@c79e@44c8@a93d@02f61270601872f65359@3dae@4d51@9fc3@0b8dcef8a3f09363ac16@bf94@46ed@aaa7@7b07f0f2e1f4cf398291@d3ca@4ad2@bb74@55de5c2e093316f4086c@bae5@4ffc@8271@039c64ec560725312768@21bf@453e@93c5@fce6e07835b02f6c626f@bfa4@4c65@9837@6fc2ff77af6b7335905c@0b61@4b79@9e5b@342a2b8d973d90e16383@6cb2@413f@9694@2442e0a55b3229c48fd9@77f2@45df@a52f@1a053c7a777d97f0a30f@6c49@4c81@b0b3@df5aa0cbbcabbee858d1@4c8d@4369@8631@9e4c504dddbf";

        public class Data
        {
            public string Roles { get; set; }
        }
        public string GenerateJwtToken(string Roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("Roles", Roles) }),
                Expires = DateTime.UtcNow.AddHours(5),
                //Expires = DateTime.UtcNow.AddSeconds(2),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
