program example ( input, output );
const c = 3;
      b = 56;
var a : 'a' .. 'c';
    k, i : integer;
begin
   read ( k, i );
   for a := 'a' to 'c' do
   case a of
       k : i := i * k;
       b : i := i + 1;
       i : k := k + 2;
       b : i := i - k;
       c : i := ( i + k ) * 2
   end;
   writeln( i, k )
end.