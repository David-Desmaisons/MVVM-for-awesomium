function OnEnter(element)
{
    $(element).addClass("boxanimated");
}

function OnClose(callback, element)
{
    $(element).removeClass("boxanimated");
	setTimeout(callback, 2000);
}