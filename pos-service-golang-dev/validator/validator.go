package validator

import (
	"github.com/go-playground/locales/en"
	ut "github.com/go-playground/universal-translator"
	"github.com/go-playground/validator/v10"
	en_translations "github.com/go-playground/validator/v10/translations/en"
)

var validate *validator.Validate
var uni *ut.UniversalTranslator

func Init() {
	en := en.New()

	validate = validator.New(validator.WithRequiredStructEnabled())
	uni = ut.New(en, en)

	trans, _ := uni.GetTranslator("en")
	en_translations.RegisterDefaultTranslations(validate, trans)
}

// Validate validates a struct and returns validation errors
func Validate(i interface{}) validator.ValidationErrors {
	if err := validate.Struct(i); err != nil {
		errs := err.(validator.ValidationErrors)
		return errs
	}
	return nil
}

func TranslateErrors(errs validator.ValidationErrors) map[string]string {
	trans, _ := uni.GetTranslator("en")
	translatedErrors := make(map[string]string)
	for _, err := range errs {
		translatedErrors[err.Field()] = err.Translate(trans)
	}
	return translatedErrors
}

// RegisterCustomValidation registers a custom validation function
func RegisterCustomValidation(tag string, fn validator.Func) error {
	return validate.RegisterValidation(tag, fn)
}
