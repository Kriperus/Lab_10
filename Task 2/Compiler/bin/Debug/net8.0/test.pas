program example ( input, output );
var
    a, b : integer;
    c : real;
    d : array[1..10] of integer;
    r : record
        x, y : integer;
        z : real;
    end;
begin
    a := 5;
    b := a + 3;
    d[1] := a;
    r.x := 10;
    with r do
        x := 20;
    begin
        c := 3.14;
        b := 100;
    end;
end.