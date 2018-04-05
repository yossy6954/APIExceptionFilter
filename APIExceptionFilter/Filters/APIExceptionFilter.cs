using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace APIExceptionFilterExample.Filters
{
    public class APIException : Exception {
        /// <summary>
        /// ステータスコード
        /// </summary>
        public int StatusCode_ = 500;

        /// <summary>
        /// 呼び出し元メソッド
        /// </summary>
        public string CallerMember;

        /// <summary>
        /// 呼び出し元ソースコード
        /// </summary>
        public string CallerFile;

        /// <summary>
        /// 呼び出し元行数
        /// </summary>
        public int CallerLine;

        /// <summary>
        /// .ctor
        /// </summary>
        public APIException() {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="memberName">呼び出し元メンバ</param>
        /// <param name="sourceFilePath">呼び出し元ファイル</param>
        /// <param name="sourceLineNumber">呼び出し元行番号</param>
        public APIException(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            )
            : base(message) {
            CallerMember = memberName;
            CallerLine = sourceLineNumber;
            try {
                CallerFile = Path.GetFileName(sourceFilePath);
            } catch (Exception) { }
        }

        /// <summary>
        /// .ctor(内部例外情報あり)
        /// </summary>
        /// <param name="inner">内部例外</param>
        /// <param name="message">メッセージ</param>
        /// <param name="memberName">呼び出し元メンバ</param>
        /// <param name="sourceFilePath">呼び出し元ファイル</param>
        /// <param name="sourceLineNumber">呼び出し元行番号</param>
        public APIException(string message, Exception inner,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            )
            : base(message, inner) {
            CallerMember = memberName;
            CallerLine = sourceLineNumber;
            try {
                CallerFile = Path.GetFileName(sourceFilePath);
            } catch (Exception) { }
        }

        /// <summary>
        /// .ctor(ステータスコードをintで設定)
        /// </summary>
        /// <param name="statusCode">ステータスコード</param>
        /// <param name="message">メッセージ</param>
        /// <param name="memberName">呼び出し元メンバ</param>
        /// <param name="sourceFilePath">呼び出し元ファイル</param>
        /// <param name="sourceLineNumber">呼び出し元行番号</param>
        public APIException(int statusCode, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            )
            : this(message, memberName, sourceFilePath, sourceLineNumber) {
            StatusCode_ = statusCode;
        }

        /// <summary>
        /// .ctor(ステータスコードをHttpStatusCodeで設定)
        /// </summary>
        /// <param name="code">ステータスコード</param>
        /// <param name="message">メッセージ</param>
        /// <param name="memberName">呼び出し元メンバ</param>
        /// <param name="sourceFilePath">呼び出し元ファイル</param>
        /// <param name="sourceLineNumber">呼び出し元行番号</param>
        public APIException(HttpStatusCode code, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            ) : this(message, memberName, sourceFilePath, sourceLineNumber) {
            StatusCode_ = (int)code;
        }

    }

    public class APIExceptionFilter : ExceptionFilterAttribute {
        private readonly ILogger Logger_;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="loggerFactory">LoggerFactory</param>
        public APIExceptionFilter(ILoggerFactory loggerFactory) {
            Logger_ = loggerFactory.CreateLogger("APIExceptionFilter");
        }

        /// <summary>
        /// 例外のキャッチ
        /// </summary>
        /// <param name="context">例外キャッチコンテキスト</param>
        public override void OnException(ExceptionContext context) {
            JsonResult result;


            if (context.Exception is APIException) {
                var ex = context.Exception as APIException;
                Logger_.LogError($"Raised APIException {ex.Message} at {ex.CallerFile}-{ex.CallerMember}({ex.CallerLine})");

                result = new JsonResult(new {
                    StatusCode = ex.StatusCode_,
                    ex.Message
                }) {
                    StatusCode = ex.StatusCode_
                };
            } else {
                string innerMsg = "";
                var ex = context.Exception;
                if (ex.InnerException != null) {
                    innerMsg = $", InnerException = {ex.InnerException.Message}";
                }

                Logger_.LogError($"Raised Exception {context.Exception.Message} - {innerMsg}]");

                result = new JsonResult(new {
                    StatusCode = 500,
                    context.Exception.Message
                }) {
                    StatusCode = 500
                };
            }

            context.Result = result;
        }
    }
}
