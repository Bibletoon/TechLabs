package com.bibletoon;

import scala.util.Right;

public class Main {

    public static void main(String[] args) {
        Object s =  Scala.checkUser(new Username("Aboba", "Abobievich"));
        System.out.println(s.getClass().getName());
        Right t = s instanceof Right ? ((Right) s) : null;
        System.out.println(t.value());
    }
}
