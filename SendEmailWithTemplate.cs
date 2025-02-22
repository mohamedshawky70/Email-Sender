
public async Task<IActionResult> Create(UserFormVM userFormVM)
{
	if (await _userManager.FindByEmailAsync(userFormVM.Email) != null)
	{
		ViewBag.Roles = _roleManager.Roles;
		TempData["ErrorMessage"] = "User with the same email that you try to add existed!";
		return View();
	}
	var result = await _userManager.CreateAsync(user, userFormVM.Password!);
	if (result.Succeeded)// UserName is Unique in table ASP.NETUser
	{
		//إختار الاسم من الرولز اللي الاي دي بتاعهم ده لكن لو برجستر مبديش لحد رول
		var roles = await _roleManager.Roles.Where(roles => userFormVM.RolesId.Contains(roles.Name!)).Select(roles => roles.Name).ToListAsync();
		await _userManager.AddToRolesAsync(user, roles!);
		//Start send email to confirm email [This code from register page]
		var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
		var callbackUrl = Url.Page(
			"/Account/ConfirmEmail",
			pageHandler: null,
			values: new { area = "Identity", userId = user.Id, code = code },
			protocol: Request.Scheme);
		//End send email to confirm email
                                                                 //static class (Const)
		var TempPath = $"{_webHostEnvironment.WebRootPath}/Templates/{EmailTemplates.Email}.html";//مكان التمبلت اللي هتتبعت
		StreamReader streamReader = new StreamReader(TempPath);//للتعامل مع هذه التمبلت
		var body = streamReader.ReadToEnd();//إقراها للاخر
		streamReader.Close();
		body = body
			.Replace("[ImgUrl]", "https://res.cloudinary.com/moshawky/image/upload/v1736097419/positive-vote_1533908_uyd3zi.png")
			.Replace("[Header]", $"Hey {user.FullName} thanks for joining us")
			.Replace("[Body]", "Please,Confirm your email")
			.Replace("[AncorUrl]", HtmlEncoder.Default.Encode(callbackUrl))
			.Replace("[AncorTitle]", "Active Account");

		BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email, "Confirm your email", body));
      //For background tasks 
      // 1) install Hangfire.AspNetCore and Hangfire.SqlServer package
      // 2) in program.cs
      //builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
      //builder.Services.AddHangfireServer();
    
			//ضيفه في الكيو علشان يشتغل مع نفسه في الباك جراوند والتطبيق يوشف اكل عيشه
      // في حالة الإشعارات هتعمل تمبلت تاني مفهاش انكور تاج
			//BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(Subscriber.Email, "Renew Message", body));
			//إبعت الإميل بعد دقيقه من الأن
			//BackgroundJob.Schedule(() =>_emailSender.SendEmailAsync(Subscriber.Email, "Renew Message", body),TimeSpan.FromMinutes(1));

		TempData["SuccessMessage"] = "Saved successfully";
	}
	else
	{
		ViewBag.Roles = _roleManager.Roles;
		TempData["ErrorMessage"] = "User name that you try to add existed!";
		return View();
	}
	return RedirectToAction(nameof(Index));
}
