#NoEnv
SendMode Input
SetWorkingDir %A_ScriptDir%
FileEncoding, UTF-16
SendMode Input
SetWorkingDir %A_ScriptDir%
#EscapeChar \
#InstallKeybdHook
#KeyHistory MaxEvents
SetBatchLines, -1
#SingleInstance Force
global flag = 0,wistle = 0,doubleff = 0,singlef = 0,swaraflag = 0,position = 0,bar := 0,oldclip := 0,kai = 0,car := 0,shiftflag = 0,vyanjanaflag = 0,capsflag = 0,special = 0,leftshiftflag = 0, rightshiftflag = 0,symbols = 0,saflag = 0,kAAA = 0,singlef1 = 0,vyanjanaflag1 = 0,kaAAA = 0,gAAA = 0,gaAAA = 0,zaAAA = 0,cAAA = 0,caAAA = 0,jAAA = 0,jaAAA = 0,zAAA = 0,qAAA = 0,qaAAA = 0,wAAA = 0,waAAA = 0,naAAA = 0,tAAA = 0,taAAA = 0,dAAA = 0,daAAA = 0,nAAA = 0,pAAA = 0,paAAA = 0,bAAA = 0,baAAA = 0,mAAA = 0,yAAA = 0,rAAA = 0,lAAA = 0,vAAA = 0,saAAA = 0,xAAA = 0,sAAA = 0,hAAA = 0,laAAA = 0,hlAAA = 0,hrAAA = 0,hrrpc = 0,hlrpc =0,keypress=0,keypress=1
Menu, Tray, NoStandard
Menu, Tray, Add , , Tex
Menu, Tray, Add , Engine Run, Res
Menu, Tray, Add , Engine Stop, Sus
Menu, Tray, Add , Exit, Exi
Menu, Tray, Icon,,, 1
Tex:
Pause
Return
Res:
Reload
Return
>#Space::
<#Space::
Sus:
Suspend, Toggle
menu, tray, ToggleCheck, Engine Stop
menu, tray, Enable, Engine Run
Return
Exi:
ExitApp
Return
#if GetKeyState("CapsLock","T")
4::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+20B9}
return
0::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0c81}
return
1::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0306}
return
2::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0304}
return
,::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0327}
return
-::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0332}
return
3::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0347}
return
5::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+030D}
return
6::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+030E}
return
7::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0323}
return
'::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0341}
return
8::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0307}
return
9::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0308}
return
.::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0324}
return
a::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0061}
return
>+A::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0041}
return
<+A::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0041}
return
b::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0062}
return
>+B::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0042}
return
<+B::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0042}
return
c::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0063}
return
>+C::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0043}
return
<+C::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0043}
return
d::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0064}
return
>+D::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0044}
return
<+D::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0044}
return
e::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0065}
return
>+E::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0045}
return
<+E::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0045}
return
f::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0066}
return
>+F::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0046}
return
<+F::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0046}
return
g::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0067}
return
>+G::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0047}
return
<+G::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0047}
return
h::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0068}
return
>+H::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0048}
return
<+H::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0048}
return
i::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0069}
return
>+I::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0049}
return
<+I::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0049}
return
j::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+006A}
return
>+J::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004A}
return
<+J::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004A}
return
k::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+006B}
return
>+K::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004B}
return
<+K::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004B}
return
l::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+006C}
return
>+L::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004C}
return
<+L::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004C}
return
m::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+006D}
return
>+M::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004D}
return
<+M::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004D}
return
n::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+006E}
return
>+N::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004E}
return
<+N::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004E}
return
o::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+006F}
return
>+O::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004F}
return
<+O::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+004F}
return
p::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0070}
return
>+P::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0050}
return
<+P::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0050}
return
q::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0071}
return
>+q::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0051}
return
<+q::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0051}
return
r::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0072}
return
>+R::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0052}
return
<+R::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0052}
return
s::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0073}
return
>+S::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0053}
return
<+S::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0053}
return
t::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0074}
return
>+T::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0054}
return
<+T::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0054}
return
u::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0075}
return
>+U::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0055}
return
<+U::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0055}
return
v::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0076}
return
>+V::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0056}
return
<+V::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0056}
return
w::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0077}
return
>+W::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0057}
return
<+W::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0057}
return
x::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0078}
return
>+X::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0058}
return
<+X::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0058}
return
y::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0079}
return
>+Y::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0059}
return
<+Y::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0059}
return
z::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+007A}
return
>+Z::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+005A}
return
<+Z::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+005A}
return
#if GetKeyState("ScrollLock","T")
1::
flagszero()
SendInput, {U+0031}
return
2::
flagszero()
SendInput, {U+0032}
return
3::
flagszero()
SendInput, {U+0033}
return
4::
flagszero()
SendInput, {U+0034}
return
5::
flagszero()
SendInput, {U+0035}
return
6::
flagszero()
SendInput, {U+0036}
return
7::
flagszero()
SendInput, {U+0037}
return
8::
flagszero()
SendInput, {U+0038}
return
9::
flagszero()
SendInput, {U+0039}
return
0::
flagszero()
SendInput, {U+0030}
return
#If
$BackSpace::
flagszero()
SendInput {BS}
return
#If GetKeyState("CapsLock","T")
>+0::
if (singlef = 1)
flagszero()
SendInput, {U+A8E0}
return
<+0::
if (singlef = 1)
flagszero()
SendInput, {U+A8E0}
return
>+1::
if (singlef = 1)
flagszero()
SendInput, {U+A8E1}
return
<+1::
if (singlef = 1)
flagszero()
SendInput, {U+A8E1}
return
>+2::
if (singlef = 1)
flagszero()
SendInput, {U+A8E2}
return
<+2::
if (singlef = 1)
flagszero()
SendInput, {U+A8E2}
return
>+3::
if (singlef = 1)
flagszero()
SendInput, {U+A8E3}
return
<+3::
if (singlef = 1)
flagszero()
SendInput, {U+A8E3}
return
>+4::
if (singlef = 1)
flagszero()
SendInput, {U+A8E4}
return
<+4::
if (singlef = 1)
flagszero()
SendInput, {U+A8E4}
return
>+5::
if (singlef = 1)
flagszero()
SendInput, {U+A8E5}
return
<+5::
if (singlef = 1)
flagszero()
SendInput, {U+A8E5}
return
>+6::
if (singlef = 1)
flagszero()
SendInput, {U+A8E6}
return
<+6::
if (singlef = 1)
flagszero()
SendInput, {U+A8E6}
return
>+7::
if (singlef = 1)
flagszero()
SendInput, {U+A8E7}
return
<+7::
if (singlef = 1)
flagszero()
SendInput, {U+A8E7}
return
>+8::
if (singlef = 1)
flagszero()
SendInput, {U+A8E8}
return
<+8::
if (singlef = 1)
flagszero()
SendInput, {U+A8E8}
return
>+9::
if (singlef = 1)
flagszero()
SendInput, {U+A8E9}
return
<+9::
if (singlef = 1)
flagszero()
SendInput, {U+A8E9}
return
>+{::
if (singlef = 1)
flagszero()
SendInput, {U+0c8c}
return
<+{::
if (singlef = 1)
flagszero()
SendInput, {U+0c8c}
return
>+}::
if (singlef = 1)
flagszero()
SendInput, {U+0ce1}
return
<+}::
if (singlef = 1)
flagszero()
SendInput, {U+0ce1}
return
<+[::
if (singlef = 1)
flagszero()
SendInput, {U+0ce2}
return
<+]::
if (singlef = 1)
flagszero()
SendInput, {U+0ce2}
return
<+!a::
if (singlef = 1)
flagszero()
SendInput, {U+0cf1}
return
>+!a::
if (singlef = 1)
flagszero()
SendInput, {U+0cf1}
return
<+!z::
if (singlef = 1)
flagszero()
SendInput, {U+0cf2}
return
>+!z::
if (singlef = 1)
flagszero()
SendInput, {U+0cf2}
return
#If
#If GetKeyState("NumLock","T")
#If
$LShift::
capsflag = 1
leftshiftflag = 1
shiftflag = 0
$LShift::LShift
return
$RShift::
capsflag = 1
rightshiftflag = 1
shiftflag = 0
$RShift::RShift
return
0::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0ce6}
return
1::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0ce7}
return
2::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0ce8}
return
3::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0ce9}
return
4::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0ceA}
return
5::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0ceb}
return
6::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0cec}
return
7::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0ced}
return
8::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0cee}
return
9::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+0cef}
return
`::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+02bb}
return
toggle := 0
'::
if (toggle = 0) {
Send, {U+2019}
toggle := 1
} else {
Send, {U+2018}
toggle := 0
}
return
,::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+002c}
return
<+`::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+007e}
return
>+`::
if (singlef = 1)
SendInput, {U+200C}
flagszero()
SendInput, {U+007e}
zwj(){
if (flag = 1 && (A_PriorHotkey = "<+f")||(A_PriorHotkey = ">+f"))
{
SendInput, {BS}{U+200D}{U+0ccd}
flag = 0
return
}
flag = 0
return
}
onef(){
if (singlef = 1)
singlef = -1
else
singlef = 0
return
}
vyanjanaflagszero(){
doubleff = 0
kai = 0
swaraflag = 0
wistle = 0
shiftflag = 0
capsflag = 0
special = 0
leftshiftflag = 0
rightshiftflag = 0
symbols = 0
saflag = 0
}
flagszero1()
{
singlef1 = 0
vyanjanaflag1 = 0
}
flagszero3()
{
hlAAA = 0
}
flagszero2()
{
kAAA = 0
kaAAA = 0
gAAA = 0
gaAAA = 0
zaAAA = 0
cAAA = 0
caAAA = 0
jAAA = 0
jaAAA = 0
zAAA = 0
qAAA = 0
qaAAA = 0
wAAA = 0
waAAA = 0
naAAA = 0
tAAA = 0
taAAA = 0
dAAA = 0
daAAA = 0
nAAA = 0
pAAA = 0
paAAA = 0
bAAA = 0
baAAA = 0
mAAA = 0
yAAA = 0
rAAA = 0
lAAA = 0
vAAA = 0
saAAA = 0
xAAA = 0
sAAA = 0
hAAA = 0
laAAA = 0
hlAAA = 0
hrAAA = 0
}
flagszero(){
doubleff = 0
kai = 0
swaraflag = 0
wistle = 0
shiftflag = 0
vyanjanaflag = 0
capsflag = 0
special = 0
leftshiftflag = 0
rightshiftflag = 0
symbols = 0
saflag = 0
flag = 0
singlef = 0
}
vyanjana(){
if (A_PriorHotKey = "r" || A_PriorHotKey = "w" || A_PriorHotKey = "<+w" || A_PriorHotKey = ">+w" || A_PriorHotKey = "q" || A_PriorHotKey = "<+q" || A_PriorHotKey = ">+q" || A_PriorHotKey = "t" || A_PriorHotKey = "<+t" || A_PriorHotKey = ">+t" || A_PriorHotKey = "y" || A_PriorHotKey = "p" || A_PriorHotKey = "<+p" || A_PriorHotKey = ">+p" || A_PriorHotKey = "s" || A_PriorHotKey = "<+s" || A_PriorHotKey = ">+s" || A_PriorHotKey = "d" || A_PriorHotKey = "<+d" || A_PriorHotKey = ">+d" || A_PriorHotKey = "g" || A_PriorHotKey = "<+g" || A_PriorHotKey = ">+g" || A_PriorHotKey = "h" || A_PriorHotKey = "j" || A_PriorHotKey = "<+j" || A_PriorHotKey = ">+j" || A_PriorHotKey = "k" || A_PriorHotKey = "<+k" || A_PriorHotKey = ">+k" || A_PriorHotKey = "l" || A_PriorHotKey = "<+l" || A_PriorHotKey = ">+l" || A_PriorHotKey = "z" || A_PriorHotKey = "<+z" || A_PriorHotKey = ">+z" || A_PriorHotKey = "x" || A_PriorHotKey = "c" || A_PriorHotKey = "<+c" || A_PriorHotKey = ">+c" || A_PriorHotKey = "v" || A_PriorHotKey = "b" || A_PriorHotKey = "<+b" || A_PriorHotKey = ">+b" || A_PriorHotKey = "n" || A_PriorHotKey = "<+n" || A_PriorHotKey = ">+n" || A_PriorHotKey = "m" || A_PriorHotKey = "<+x" || A_PriorHotKey = ">+x" || singlef = 1 || (A_PriorHotKey = "<+x" && symbols = 0) || (A_PriorHotKey = ">+x" && symbols = 0))
{
return 1
}
else
return 0
}
funsinglef(){
if (singlef = 1)
{
SendInput, {BS}
singlef = 0
}
return
}
r::
if (singlef1 = 1)
{
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB0}
hrrpc = 1
vyanjanaflag1 = 1
return
}
if (singlef = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CC3}
flagszero()
special = 1
return
}
if (doubleff = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C8B}
flagszero()
special = 1
wistle = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C8B}
flagszero()
special = 1
wistle = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 1)
{
flagszero2()
SendInput, {U+0CC3}
flagszero()
vyanjanaflag1 = 1
special = 1
return
}
if (singlef = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CC3}
flagszero()
special = 1
return
}
if (doubleff = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C8B}
flagszero()
special = 1
wistle = 1
return
}
if (capsflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C8B}
flagszero()
special = 1
wistle = 1
return
}
if (capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CC3}
flagszero()
special = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB0}
rAAA = 1
return
a::
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CBE}
flagszero()
vyanjanaflag1 = 1
return
}
if (((A_PriorHotKey != "r") && (A_PriorHotKey != "w") && (A_PriorHotKey != ">+w")  && (A_PriorHotKey != "<+w") && (A_PriorHotKey != "q") && (A_PriorHotKey != ">+q") && (A_PriorHotKey != "<+q") && (A_PriorHotKey != "t") && (A_PriorHotKey != ">+t") && (A_PriorHotKey != "<+t") && (A_PriorHotKey != "y") && (A_PriorHotKey != "p") && (A_PriorHotKey != ">+p") && (A_PriorHotKey != "<+p") && (A_PriorHotKey != "s") && (A_PriorHotKey != ">+s") && (A_PriorHotKey != "<+s") && (A_PriorHotKey != "d") && (A_PriorHotKey != ">+d") && (A_PriorHotKey != "<+d") && (A_PriorHotKey != "g") && (A_PriorHotKey != ">+g") && (A_PriorHotKey != "<+g") && (A_PriorHotKey != "h") && (A_PriorHotKey != "j") && (A_PriorHotKey != ">+j") && (A_PriorHotKey != "<+j") && (A_PriorHotKey != "k") && (A_PriorHotKey != ">+k") && (A_PriorHotKey != "<+k") && (A_PriorHotKey != "l") && (A_PriorHotKey != ">+l") && (A_PriorHotKey != "z") && (A_PriorHotKey != ">+z") && (A_PriorHotKey != "<+z") && (A_PriorHotKey != "x") && (A_PriorHotKey != "c") && (A_PriorHotKey != ">+c") && (A_PriorHotKey != "<+c") && (A_PriorHotKey != "v") && (A_PriorHotKey != "b") && (A_PriorHotKey != ">+b") && (A_PriorHotKey != "<+b") && (A_PriorHotKey != "n") && (A_PriorHotKey != ">+n") && (A_PriorHotKey != "<+n") && (A_PriorHotKey != "m") && (A_PriorHotKey != "x") && (A_PriorHotkey != ">+l")  && (A_PriorHotkey != "<+l") && (singlef = 0)) || (doubleff =1))
{
SendInput, {U+0C85}
flagszero()
return
}
funsinglef()
flagszero()
return
s::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB8}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
saflag = 1
SendInput, {U+0CB8}
sAAA = 1
return
d::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA6}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA6}
dAAA = 1
return
w::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA1}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA1}
wAAA = 1
return
q::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9F}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9f}
qAAA = 1
return
t::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA4}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA4}
tAAA = 1
return
y::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAF}
vyanjanaflag1 = 1
return
}
if (singlef = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CC8}
flagszero()
special = 1
return
}
if (doubleff = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C90}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C90}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CC8}
flagszero()
special = 1
return
}
if (singlef = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CC8}
flagszero()
special = 1
return
}
if (doubleff = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C90}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C90}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CC8}
flagszero()
special = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAF}
yAAA = 1
return
p::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAA}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAA}
pAAA = 1
return
g::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C97}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C97}
gAAA = 1
return
h::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB9}
vyanjanaflag1 = 1
return
}
if (singlef = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}
flagszero()
special = 1
return
}
if (doubleff = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C83}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C83}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C83}
flagszero()
special = 1
return
}
if (singlef = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}
flagszero()
special = 1
return
}
if (doubleff = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C83}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C83}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C83}
flagszero()
special = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB9}
hAAA = 1
return
j::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9C}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9C}
jAAA = 1
return
l::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB2}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB2}
lAAA = 1
return
k::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C95}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C95}
vyanjanaflag1 = 1
kAAA = 1
return
z::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9E}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9E}
zAAA = 1
return
x::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB7}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB7}
xAAA = 1
return
c::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9A}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9A}
cAAA = 1
return
v::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB5}
vyanjanaflag1 = 1
return
}
if (singlef = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CCC}
flagszero()
special = 1
return
}
if (doubleff = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C94}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C94}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CCC}
flagszero()
special = 1
return
}
if (singlef = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CCC}
flagszero()
special = 1
return
}
if (doubleff = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C94}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C94}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CCC}
flagszero()
special = 1
return
}
zwj()
onef()
if ((saflag = 1) && (A_PriorHotkey = "f"))
{
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB5}
return
}
flagszero1()
flagszero2()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB5}
vAAA = 1
return
b::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAC}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAC}
bAAA = 1
return
n::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA8}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA8}
nAAA = 1
return
m::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAE}
vyanjanaflag1 = 1
return
}
if (singlef = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}
flagszero()
special = 1
return
}
if (doubleff = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C82}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C82}
flagszero()
special = 1
return
}
if (capsflag = 1)
{
SendInput, {U+0C82}
flagszero()
special = 1
return
}
if (shiftflag = 1)
{
SendInput, {U+0CAE}{U+0CCD}{U+0CAE}
vyanjanaflagszero()
vyanjanaflag = 1
return
}
if (singlef = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}
flagszero()
special = 1
return
}
if (doubleff = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C82}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C82}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C82}
flagszero()
special = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAE}
mAAA = 1
return
>+q::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA0}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA0}
qaAAA = 1
return
<+q::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA0}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA0}
qaAAA = 1
return
>+w::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA2}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA2}
waAAA = 1
return
<+w::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA2}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA2}
waAAA = 1
return
>+t::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA5}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA5}
taAAA = 1
return
<+t::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA5}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA5}
taAAA = 1
return
>+p::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAB}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAB}
paAAA = 1
return
<+p::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAB}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAB}
paAAA = 1
return
>+s::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB6}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB6}
saAAA = 1
return
<+s::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB6}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB6}
saAAA = 1
return
>+d::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA7}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA7}
daAAA = 1
return
<+d::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA7}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA7}
daAAA = 1
return
>+g::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C98}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C98}
gaAAA = 1
return
<+g::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C98}
vyanjanaflag1 = 1
return
}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C98}
gaAAA = 1
return
>+j::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9D}
vyanjanaflag1 = 1
return
}
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9D}
gaAAA = 1
return
<+j::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9D}
vyanjanaflag1 = 1
return
}
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9D}
gaAAA = 1
return
>+k::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C96}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C96}
kaAAA = 1
return
<+k::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C96}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C96}
kaAAA = 1
return
>+l::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB3}
hlrpc =1
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB3}
laAAA = 1
return
<+l::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB3}
hlrpc = 1
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CB3}
laAAA = 1
return
>+z::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C99}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C99}
zaAAA = 1
return
<+z::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C99}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C99}
zaAAA = 1
return
>+c::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9B}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9B}
caAAA = 1
return
<+c::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9B}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0C9B}
caAAA = 1
return
>+b::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAD}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAD}
baAAA = 1
return
<+b::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAD}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CAD}
baAAA = 1
return
>+n::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA3}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA3}
naAAA = 1
return
<+n::
if (singlef1 = 1)
{
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA3}
vyanjanaflag1 = 1
return
}
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {U+0CA3}
naAAA = 1
return
i::
if (A_PriorHotkey = "m" && special = 1 && capsflag = 1)
{
SendInput, {U+0C88}
flagszero()
return
}
if (A_PriorHotkey = "m" && special = 1)
{
SendInput, {U+0C87}
flagszero()
return
}
if (A_PriorHotkey = "h" && special = 1 && capsflag = 1)
{
SendInput, {U+0C88}
flagszero()
return
}
if (A_PriorHotkey = "h" && special = 1)
{
SendInput, {U+0C87}
flagszero()
return
}
if (singlef = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CBF}{U+0CD5}
flagszero()
special = 1
return
}
if (doubleff = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C88}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C88}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CC0}
flagszero()
special = 1
return
}
if (singlef = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CBF}{U+0CD5}
flagszero()
special = 1
return
}
if (doubleff = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C88}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C88}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CC0}
flagszero()
special = 1
return
}
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CBF}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CBF}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C87}
return
o::
if (A_PriorHotkey = "m" && special = 1 && capsflag = 1)
{
SendInput, {U+0C93}
flagszero()
return
}
if (A_PriorHotkey = "m" && special = 1 && shiftflag = 1)
{
SendInput, {U+0C93}
flagszero()
return
}
if (A_PriorHotkey = "m" && special = 1)
{
SendInput, {U+0C92}
flagszero()
return
}
if (A_PriorHotkey = "h" && special = 1 && capsflag = 1)
{
SendInput, {U+0C93}
flagszero()
return
}
if (A_PriorHotkey = "h" && special = 1 && shiftflag = 1)
{
SendInput, {U+0C93}
flagszero()
return
}
if (A_PriorHotkey = "h" && special = 1)
{
SendInput, {U+0C92}
flagszero()
return
}
if (singlef = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CCA}{U+0CD5}
flagszero()
special = 1
return
}
if (doubleff = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C93}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C93}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CCB}
flagszero()
special = 1
return
}
if (singlef = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CCA}{U+0CD5}
flagszero()
special = 1
return
}
if (doubleff = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C93}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C93}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CCB}
flagszero()
special = 1
return
}
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CCA}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CCA}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C92}
return
e::
if (A_PriorHotkey = "m" && special = 1 && capsflag = 1)
{
SendInput, {U+0C8F}
flagszero()
return
}
if (A_PriorHotkey = "m" && special = 1)
{
SendInput, {U+0C8E}
flagszero()
return
}
if (A_PriorHotkey = "h" && special = 1 && capsflag = 1)
{
SendInput, {U+0C8F}
flagszero()
return
}
if (A_PriorHotkey = "h" && special = 1)
{
SendInput, {U+0C8E}
flagszero()
return
}
if (singlef = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CC6}{U+0CD5}
flagszero()
special = 1
return
}
if (doubleff = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C8F}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C8F}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CC7}
flagszero()
special = 1
return
}
if (singlef = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CC6}{U+0CD5}
flagszero()
special = 1
return
}
if (doubleff = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C8F}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C8F}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CC7}
flagszero()
special = 1
return
}
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CC6}
flagszero()
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC6}
flagszero()
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C8E}
return
u::
if (A_PriorHotkey = "m" && special = 1 && capsflag = 1)
{
SendInput, {U+0C8A}
flagszero()
return
}
if (A_PriorHotkey = "m" && special = 1)
{
SendInput, {U+0C89}
flagszero()
return
}
if (A_PriorHotkey = "h" && special = 1 && capsflag = 1)
{
SendInput, {U+0C8A}
flagszero()
return
}
if (A_PriorHotkey = "h" && special = 1)
{
SendInput, {U+0C89}
flagszero()
return
}
if (singlef = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CC2}
flagszero()
special = 1
return
}
if (doubleff = 1 && shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C8A}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C8A}
flagszero()
special = 1
return
}
if (shiftflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CC2}
flagszero()
special = 1
return
}
if (singlef = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {BS}{U+0CC2}
flagszero()
special = 1
return
}
if (doubleff = 1 && capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0C8A}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 0)
{
SendInput, {U+0C8A}
flagszero()
special = 1
return
}
if (capsflag = 1 && vyanjanaflag = 1)
{
SendInput, {U+0CC2}
flagszero()
special = 1
return
}
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CC1}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC1}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C89}
return
>+h::
if (singlef = 1)
{
SendInput, {BS}
flagszero()
return
}
flagszero()
SendInput, {U+0C83}
return
<+h::
if (singlef = 1)
{
SendInput, {BS}
flagszero()
return
}
flagszero()
SendInput, {U+0C83}
return
>+m::
if (singlef = 1)
{
SendInput, {BS}
flagszero()
return
}
flagszero()
SendInput, {U+0C82}
return
<+m::
if (singlef = 1)
{
SendInput, {BS}
flagszero()
return
}
flagszero()
SendInput, {U+0C82}
return
>+e::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CC7}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC7}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C8F}
return
<+e::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CC7}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC7}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C8F}
return
>+y::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CC8}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC8}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C90}
return
<+y::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CC8}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC8}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C90}
return
>+u::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CC2}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC2}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C8A}
return
<+u::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CC2}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC2}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C8A}
return
>+i::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CC0}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC0}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C88}
return
<+i::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CC0}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC0}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C88}
return
>+o::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CCB}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CCB}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C93}
return
<+o::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CCB}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CCB}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C93}
return
>+a::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CBE}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CBE}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C86}
return
<+a::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CBE}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CBE}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C86}
return
>+r::
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC3}
flagszero()
vyanjanaflag1 = 1
swaraflag = 1
return
}
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
SendInput, {U+0CC3}
flagszero()
return
}
if (A_PriorHotKey != "r" && A_PriorHotKey != "w" && A_PriorHotKey != "<+w" && A_PriorHotKey != ">+w" && A_PriorHotKey != "q" && A_PriorHotKey != "<+q" && A_PriorHotKey != ">+q" && A_PriorHotKey != "t" && A_PriorHotKey != "<+t" && A_PriorHotKey != ">+t" && A_PriorHotKey != "y" && A_PriorHotKey != "p" && A_PriorHotKey != "<+p" && A_PriorHotKey != ">+p" && A_PriorHotKey != "s" && A_PriorHotKey != "<+s" && A_PriorHotKey != ">+s" && A_PriorHotKey != "d" && A_PriorHotKey != "<+d" && A_PriorHotKey != ">+d" && A_PriorHotKey != "g" && A_PriorHotKey != "<+g" && A_PriorHotKey != ">+g" && A_PriorHotKey != "h" && A_PriorHotKey != "j" && A_PriorHotKey != "<+j" && A_PriorHotKey != ">+j" && A_PriorHotKey != "k" && A_PriorHotKey != "<+k" && A_PriorHotKey != ">+k" && A_PriorHotKey != "l" && A_PriorHotKey != "<+l" && A_PriorHotKey != ">+l" && A_PriorHotKey !="x" && A_PriorHotKey != "c" && A_PriorHotKey != "<+c" && A_PriorHotKey != ">+c" && A_PriorHotKey != "v" && A_PriorHotKey != "b" && A_PriorHotKey != "<+b" && A_PriorHotKey != ">+b" && A_PriorHotKey != "n" && A_PriorHotKey != "<+n" && A_PriorHotKey != ">+n" && A_PriorHotKey != "m" && A_PriorHotKey != "z" && A_PriorHotKey != "<+z" && A_PriorHotKey != ">+z")
{
flagszero()
wistle = 1
SendInput, {U+0C8b}
return
}
<+r::
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CC3}
flagszero()
vyanjanaflag1 = 1
swaraflag = 1
return
}
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
SendInput, {U+0CC3}
flagszero()
return
}
if (A_PriorHotKey != "r" && A_PriorHotKey != "w" && A_PriorHotKey != "<+w" && A_PriorHotKey != ">+w" && A_PriorHotKey != "q" && A_PriorHotKey != "<+q" && A_PriorHotKey != ">+q" && A_PriorHotKey != "t" && A_PriorHotKey != "<+t" && A_PriorHotKey != ">+t" && A_PriorHotKey != "y" && A_PriorHotKey != "p" && A_PriorHotKey != "<+p" && A_PriorHotKey != ">+p" && A_PriorHotKey != "s" && A_PriorHotKey != "<+s" && A_PriorHotKey != ">+s" && A_PriorHotKey != "d" && A_PriorHotKey != "<+d" && A_PriorHotKey != ">+d" && A_PriorHotKey != "g" && A_PriorHotKey != "<+g" && A_PriorHotKey != ">+g" && A_PriorHotKey != "h" && A_PriorHotKey != "j" && A_PriorHotKey != "<+j" && A_PriorHotKey != ">+j" && A_PriorHotKey != "k" && A_PriorHotKey != "<+k" && A_PriorHotKey != ">+k" && A_PriorHotKey != "l" && A_PriorHotKey != "<+l" && A_PriorHotKey != ">+l" && A_PriorHotKey !="x" && A_PriorHotKey != "c" && A_PriorHotKey != "<+c" && A_PriorHotKey != ">+c" && A_PriorHotKey != "v" && A_PriorHotKey != "b" && A_PriorHotKey != "<+b" && A_PriorHotKey != ">+b" && A_PriorHotKey != "n" && A_PriorHotKey != "<+n" && A_PriorHotKey != ">+n" && A_PriorHotKey != "m" && A_PriorHotKey != "z" && A_PriorHotKey != "<+z" && A_PriorHotKey != ">+z")
{
flagszero()
wistle = 1
SendInput, {U+0C8b}
return
}
>+x::
if(A_PriorHotKey = "o" && swaraflag = 1)
{
SendInput,{BS}{U+0AD0}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflagszero()
singlef = 2
return
}
if((A_PriorHotKey = "<+r" || A_PriorHotKey = ">+r") && special = 1 && wistle = 0)
{
SendInput, {BS}{U+0CC4}
flagszero()
symbols = 1
return
}
if ((A_PriorHotKey = "<+r" || A_PriorHotKey = ">+r") && (wistle = 0))
{
SendInput, {BS}{U+0CC4}
flagszero()
symbols = 1
return
}
if ((A_PriorHotkey = "<+r") || (A_PriorHotkey = ">+r") && (if A_PriorHotKey != "r" || A_PriorHotKey != "w" || A_PriorHotKey != "<+w" || A_PriorHotKey != ">+w" || A_PriorHotKey != "q" || A_PriorHotKey != "<+q" || A_PriorHotKey != ">+q" || A_PriorHotKey != "t" || A_PriorHotKey != "<+t" || A_PriorHotKey != ">+t" || A_PriorHotKey != "y" || A_PriorHotKey != "p" || A_PriorHotKey != "<+p" || A_PriorHotKey != ">+p" || A_PriorHotKey != "s" || A_PriorHotKey != "<+s" || A_PriorHotKey != ">+s" || A_PriorHotKey != "d" || A_PriorHotKey != "<+d" || A_PriorHotKey != ">+d" || A_PriorHotKey != "g" || A_PriorHotKey != "<+g" || A_PriorHotKey != ">+g" || A_PriorHotKey != "h" || A_PriorHotKey != "j" || A_PriorHotKey != "<+j" || A_PriorHotKey != ">+j" || A_PriorHotKey != "k" || A_PriorHotKey != "<+k" || A_PriorHotKey != ">+k" || A_PriorHotKey != "l" || A_PriorHotKey != "<+l" || A_PriorHotKey != ">+l" || A_PriorHotKey != "z" || A_PriorHotKey != "<+z" || A_PriorHotKey != ">+z" || A_PriorHotKey != "x" || A_PriorHotKey != "c" || A_PriorHotKey != "<+c" || A_PriorHotKey != ">+c" || A_PriorHotKey != "v" || A_PriorHotKey != "b" || A_PriorHotKey != "<+b" || A_PriorHotKey != ">+b" || A_PriorHotKey != "n" || A_PriorHotKey != "<+n" || A_PriorHotKey != ">+n" || A_PriorHotKey != "m" || A_PriorHotKey != "<+x" || A_PriorHotKey != ">+x"))
{
SendInput, {BS}{U+0CE0}
flagszero()
symbols = 1
return
}
if(singlef1 = 1)
{
flagszero3()
flagszero2()
flagszero1()
SendInput, {U+0CDE}
flagszero()
vyanjanaflag1 = 1
return
}
if (A_PriorHotKey = "r")
{
if (singlef = 1)
{
flagszero()
return
}
SendInput, {BS}{U+0CB1}
flagszero()
hrAAA = 1
return
}
if (A_PriorHotKey = ">+l" || A_PriorHotKey = "<+l")
{
flagszero2()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {BS}{U+0CDE}
flagszero()
hlAAA = 1
return
}
if (A_PriorHotKey = "s" && special = 0)
{
SendInput, {BS}{U+0CBD}
flagszero()
symbols = 1
return
}
SendInput, {U+0CBC}
flagszero()
return
<+x::
if(A_PriorHotKey = "o" && swaraflag = 1)
{
SendInput,{BS}{U+0AD0}
flagszero1()
flagszero2()
zwj()
onef()
vyanjanaflagszero()
vyanjanaflagszero()
singlef = 2
return
}
if((A_PriorHotKey = "<+r" || A_PriorHotKey = ">+r") && special = 1 && wistle = 0)
{
SendInput, {BS}{U+0CC4}
flagszero()
symbols = 1
return
}
if ((A_PriorHotKey = "<+r" || A_PriorHotKey = ">+r") && (wistle = 0))
{
SendInput, {BS}{U+0CC4}
flagszero()
symbols = 1
return
}
if ((A_PriorHotkey = "<+r") || (A_PriorHotkey = ">+r") && (if A_PriorHotKey != "r" || A_PriorHotKey != "w" || A_PriorHotKey != "<+w" || A_PriorHotKey != ">+w" || A_PriorHotKey != "q" || A_PriorHotKey != "<+q" || A_PriorHotKey != ">+q" || A_PriorHotKey != "t" || A_PriorHotKey != "<+t" || A_PriorHotKey != ">+t" || A_PriorHotKey != "y" || A_PriorHotKey != "p" || A_PriorHotKey != "<+p" || A_PriorHotKey != ">+p" || A_PriorHotKey != "s" || A_PriorHotKey != "<+s" || A_PriorHotKey != ">+s" || A_PriorHotKey != "d" || A_PriorHotKey != "<+d" || A_PriorHotKey != ">+d" || A_PriorHotKey != "g" || A_PriorHotKey != "<+g" || A_PriorHotKey != ">+g" || A_PriorHotKey != "h" || A_PriorHotKey != "j" || A_PriorHotKey != "<+j" || A_PriorHotKey != ">+j" || A_PriorHotKey != "k" || A_PriorHotKey != "<+k" || A_PriorHotKey != ">+k" || A_PriorHotKey != "l" || A_PriorHotKey != "<+l" || A_PriorHotKey != ">+l" || A_PriorHotKey != "z" || A_PriorHotKey != "<+z" || A_PriorHotKey != ">+z" || A_PriorHotKey != "x" || A_PriorHotKey != "c" || A_PriorHotKey != "<+c" || A_PriorHotKey != ">+c" || A_PriorHotKey != "v" || A_PriorHotKey != "b" || A_PriorHotKey != "<+b" || A_PriorHotKey != ">+b" || A_PriorHotKey != "n" || A_PriorHotKey != "<+n" || A_PriorHotKey != ">+n" || A_PriorHotKey != "m" || A_PriorHotKey != "<+x" || A_PriorHotKey != ">+x"))
{
SendInput, {BS}{U+0CE0}
flagszero()
symbols = 1
return
}
if(singlef1 = 1)
{
flagszero3()
flagszero2()
flagszero1()
SendInput, {U+0CDE}
flagszero()
vyanjanaflag1 = 1
return
}
if (A_PriorHotKey = "r")
{
if (singlef = 1)
{
flagszero()
return
}
SendInput, {BS}{U+0CB1}
flagszero()
hrAAA = 1
return
}
if (A_PriorHotKey = ">+l" || A_PriorHotKey = "<+l")
{
flagszero2()
vyanjanaflagszero()
vyanjanaflag = 1
SendInput, {BS}{U+0CDE}
flagszero()
hlAAA = 1
return
}
if (A_PriorHotKey = "s" && special = 0)
{
SendInput, {BS}{U+0CBD}
flagszero()
symbols = 1
return
}
SendInput, {U+0CBC}
flagszero()
return
>+v::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CCC}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CCC}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C94}
return
<+v::
if ((vyanjanaflag = 1) && (singlef = 0 || singlef = -1))
{
flagszero2()
funsinglef()
SendInput, {U+0CCC}
flagszero()
vyanjanaflag1 = 1
return
}
if (vyanjana())
{
flagszero2()
funsinglef()
SendInput, {U+0CCC}
flagszero()
vyanjanaflag1 = 1
return
}
flagszero()
swaraflag = 1
SendInput, {U+0C94}
return
$Tab::
if (singlef = 1)
SendInput, {U+200C}
SendInput, {Tab}
flagszero()
return
$Space::
if (singlef = 1)
SendInput, {U+200C}
SendInput, {Space}
flagszero()
return
$Enter::
if (singlef = 1)
SendInput, {U+200C}
SendInput, {Enter}
flagszero()
return
>+F::
if(vyanjanaflag = 1)
{
flagszero2()
SendInput, {left 3}{bs}{left 0}{U+0ccd}{Right 5}
flagszero1()
return
}
<+F::
if (A_Priorhotkey = "r" && singlef = 0)
{
flagszero()
singlef = 1
flag = 1
SendInput, {U+0ccd}
return
}
return
f::
if (capsflag = 1 || leftshiftflag = 1 || rightshiftflag = 1 || shiftflag = 1)
{
flagszero()
return
}
if (A_PriorHotkey = "a" || if A_PriorHotKey = "<+a" || if A_PriorHotKey = ">+a" || A_PriorHotKey = "e" || A_PriorHotKey = "<+e" || A_PriorHotKey = ">+e" || A_PriorHotKey = "<+r" || A_PriorHotKey = ">+r" || A_PriorHotKey = "<+y" || A_PriorHotKey = ">+y" || A_PriorHotKey = "u" || A_PriorHotKey = "<+u" || A_PriorHotKey = ">+u" || A_PriorHotKey = "i" || A_PriorHotKey = "<+i" || A_PriorHotKey = ">+i" || A_PriorHotKey = "o" || A_PriorHotKey = "<+o" || A_PriorHotKey = ">+o" || A_PriorHotKey = "<+h" || A_PriorHotKey = ">+h" || A_PriorHotKey = "<+m" || A_PriorHotKey = ">+m" || kai = 1 || (A_PriorHotkey = "r" && special = 1) || (A_PriorHotkey = "y" && special = 1) || (A_PriorHotkey = "v" && special = 1) || (A_PriorHotkey = "m" && special = 1) || (A_PriorHotkey = "h" && special = 1))
{
flagszero()
return
}
if (A_PriorHotkey = "f" && doubleff = 1)
{
flagszero()
return
}
if (A_PriorHotKey = "f" && doubleff = 0 && singlef = 1)
{
SendInput, {U+200C}
flagszero()
doubleff = 1
return
}
if (A_Priorhotkey = "r" && singlef = 0)
{
flagszero()
singlef = 1
flag = 1
SendInput, {U+0ccd}
return
}
if(hlAAA = 1)
{
SendInput, {U+0ccd}
singlef1 = 1
doubleff = 0
shiftflag = 0
capsflag = 0
rightshiftflag = 0
leftshiftflag = 0
return
}
if(hrAAA = 1)
{
SendInput, {U+0ccd}
singlef = 1
doubleff = 0
shiftflag = 0
capsflag = 0
rightshiftflag = 0
leftshiftflag = 0
return
}
if (A_PriorHotKey = "r" || A_PriorHotKey = "w" || A_PriorHotKey = "<+w" || A_PriorHotKey = ">+w" || A_PriorHotKey = "q" || A_PriorHotKey = "<+q" ||A_PriorHotKey = ">+q" || A_PriorHotKey = "t" || A_PriorHotKey = "<+t" || A_PriorHotKey = ">+t" || A_PriorHotKey = "y" || A_PriorHotKey = "p" || A_PriorHotKey = "<+p" || A_PriorHotKey = ">+p" || A_PriorHotKey = "s" || A_PriorHotKey = "<+s" || A_PriorHotKey = ">+s" || A_PriorHotKey = "d" || A_PriorHotKey = "<+d" || A_PriorHotKey = ">+d" || A_PriorHotKey = "g" || A_PriorHotKey = "<+g" || A_PriorHotKey = ">+g" || A_PriorHotKey = "h" || A_PriorHotKey = "j" || A_PriorHotKey = "<+j" || A_PriorHotKey = ">+j" || A_PriorHotKey = "k" || A_PriorHotKey = "<+k" || A_PriorHotKey = ">+k" || A_PriorHotKey = "l" || A_PriorHotKey = "<+l" || A_PriorHotKey = ">+l" || A_PriorHotKey = "z" || A_PriorHotKey = "<+z" || A_PriorHotKey = ">+z" || A_PriorHotKey = "x" || A_PriorHotKey = "c" || A_PriorHotKey = "<+c" || A_PriorHotKey = ">+c" || A_PriorHotKey = "v" || A_PriorHotKey = "b" || A_PriorHotKey = "<+b" || A_PriorHotKey = ">+b" || A_PriorHotKey = "n" || A_PriorHotKey = "<+n" || A_PriorHotKey = ">+n" || A_PriorHotKey = "m")
{
SendInput, {U+0ccd}
singlef = 1
doubleff = 0
shiftflag = 0
capsflag = 0
rightshiftflag = 0
leftshiftflag = 0
return
}
flagszero()
