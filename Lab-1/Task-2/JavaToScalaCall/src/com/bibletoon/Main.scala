package com.bibletoon;

sealed trait Expr[A]
case class Username(firstName : String, secondName: String) extends Expr[String]
case class Email(value : String) extends Expr[String]

object Scala {
  def checkUser[A](id: Expr[A]) = id match {
    case Username(firstName, secondName) => Left(secondName+" "+firstName)
    case Email(email) => Right(email.reverse)
  }

  def main(args: Array[String]) = {
    checkUser(Username("aboba", "abobievich")) match {
      case Left(value) => println(value)
      case Right(value) => println(value)
    }
  }
}