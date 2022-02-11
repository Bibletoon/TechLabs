var dto = new FsProgram.UserDto("bibletoon", "Bible Toon", FsProgram.Country.Canada);

var user = FsProgram.createUserFromDto(dto);

Console.WriteLine(user.IsOk ? user.ResultValue.Username : user.ErrorValue.First());