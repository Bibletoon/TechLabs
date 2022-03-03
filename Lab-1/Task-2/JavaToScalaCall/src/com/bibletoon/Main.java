package com.bibletoon;

import scala.util.Left;
import scala.util.Right;

public class Main {

    public static void main(String[] args) {
        Object s =  Scala.checkUser(new Username("Aboba", "Abobievich"));
        System.out.println(s.getClass().getName());
        var t = s instanceof Right ? ((Right) s).value() : ((Left) s).value();
        System.out.println(t);
    }
}
