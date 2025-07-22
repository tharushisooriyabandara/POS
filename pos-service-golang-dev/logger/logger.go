package logger

import (
	"os"
	"strconv"

	"github.com/Delivergate-Dev/pos-service-golang/config"
	"go.uber.org/zap"
	"go.uber.org/zap/zapcore"
	"gopkg.in/natefinch/lumberjack.v2"
)

var log *zap.Logger

func Init(cgf *config.Config) {
	maxSize, _ := strconv.Atoi(cgf.ErrorLogMaxSize)
	maxAge, _ := strconv.Atoi(cgf.ErrorLogMaxAge)
	maxBackups, _ := strconv.Atoi(cgf.ErrorLogMaxBackups)
	shouldCompress, _ := strconv.ParseBool(cgf.ErrorLogCompress)

	// encoders
	consoleEncoder := zapcore.NewConsoleEncoder(zap.NewDevelopmentEncoderConfig())
	jsonEncoder := zapcore.NewJSONEncoder(zap.NewProductionEncoderConfig())

	// write syncs
	stdoutSyncer := zapcore.Lock(os.Stdout)
	fileSyncer := zapcore.AddSync(&lumberjack.Logger{
		Filename:   cgf.ErrorLogFile,
		MaxSize:    maxSize,
		MaxBackups: maxBackups,
		MaxAge:     maxAge,
		Compress:   shouldCompress,
	})

	// levels
	infoLevel := zap.LevelEnablerFunc(func(lvl zapcore.Level) bool {
		return lvl >= zapcore.InfoLevel && lvl < zapcore.ErrorLevel
	})
	errorLevel := zap.LevelEnablerFunc(func(lvl zapcore.Level) bool {
		return lvl >= zapcore.ErrorLevel
	})

	// cores
	infoCore := zapcore.NewCore(consoleEncoder, stdoutSyncer, infoLevel)
	errorCore := zapcore.NewCore(jsonEncoder, fileSyncer, errorLevel)
	core := zapcore.NewTee(infoCore, errorCore)

	log = zap.New(core)

}

func Info(msg string, fields ...zap.Field) {
	log.Info(msg, fields...)
}

func Error(msg string, fields ...zap.Field) {
	log.Error(msg, fields...)
}

func Debug(msg string, fields ...zap.Field) {
	log.Debug(msg, fields...)
}

func Fatal(msg string, fields ...zap.Field) {
	log.Fatal(msg, fields...)
}

func With(fields ...zap.Field) *zap.Logger {
	return log.With(fields...)
}
