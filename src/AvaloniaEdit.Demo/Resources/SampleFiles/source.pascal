program reverse_number;
var x,y : longint;
begin
  write('Enter a number to reverse: ');
  read(x);
  while (x<>0) do
    begin
      y:= x mod 10;
      write(y);
      x := x div 10;
    end;
end.