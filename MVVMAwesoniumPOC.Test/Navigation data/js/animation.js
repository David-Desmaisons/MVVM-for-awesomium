var b = $("#BB");


function OnEnter()
{
	b.addClass("boxanimated");
}

function OnClose(callback)
{
	b.removeClass("boxanimated");
	setTimeout(callback, 3000);
}