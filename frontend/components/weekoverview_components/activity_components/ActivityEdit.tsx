import { zodResolver } from "@hookform/resolvers/zod";
import { router } from "expo-router";
import React, { useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import useActivity, { ActivityDTO, FullActivityDTO } from "../../../hooks/useActivity";
import { useDate } from "../../../providers/DateProvider";
import { useToast } from "../../../providers/ToastProvider";
import formatTimeHHMM from "../../../utils/formatTimeHHMM";
import FormContainer from "../../forms/FormContainer";
import FormHeader from "../../forms/FormHeader";
import FormTimePicker from "../../forms/FormTimePicker";
import SubmitButton from "../../forms/SubmitButton";
import dateAndTimeToISO from "../../../utils/dateAndTimeToISO";
import { useWeekplan } from "../../../providers/WeekplanProvider";
import PictogramSelector from "../../PictogramSelector";
import { colors, ScaleSizeH, ScaleSizeW } from "../../../utils/SharedStyles";
import { Keyboard, ScrollView, TouchableWithoutFeedback, Image } from "react-native";
import ProgressSteps, { ProgressStepsMethods } from "../../ProgressSteps";
import SecondaryButton from "../../forms/SecondaryButton";
import { pictogramImageUrl } from "../../../utils/globals";
import { usePictogramById } from "../../../hooks/usePictogramById";
import SafeArea from "../../SafeArea";

const schema = z
  .object({
    startTime: z.date(),
    endTime: z.date(),
    date: z.date(),
    pictogramId: z.number(),
  })
  .superRefine((data, ctx) => {
    if (data.startTime > data.endTime) {
      ctx.addIssue({
        code: z.ZodIssueCode.invalid_date,
        path: ["endTime"],
        message: "Sluttidspunktet skal være efter starttidspunktet",
      });
    }
  });

type FormData = z.infer<typeof schema>;

/**
 * Component for editing an activity.
 *
 * @component
 * @param {string} title - The title of the activity.
 * @param {string} description - The description of the activity.
 * @param {Date} startTime - The start time of the activity.
 * @param {Date} endTime - The end time of the activity.
 * @param {number} activityId - The id of the activity.
 * @param {boolean} isCompleted - Whether the activity is completed.
 * @returns {JSX.Element}
 * @example
 * <ActivityEdit
 *   title="Meeting"
 *   description="Discuss project updates"
 *   startTime={new Date()}
 *   endTime={new Date()}
 *   activityId="12345"
 *   isCompleted={false}
 * />
 */
const ActivityEdit = ({ activity }: { activity: ActivityDTO }) => {
  const { selectedDate } = useDate();
  const { id } = useWeekplan();
  const { updateActivity } = useActivity({ date: selectedDate });
  const { addToast } = useToast();
  const progressRef = useRef<ProgressStepsMethods>(null);
  const { data: existingPictogram } = usePictogramById(activity.pictogramId);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const displayUrl = previewUrl ?? pictogramImageUrl(existingPictogram?.pictogramUrl);

  const {
    control,
    setValue,
    handleSubmit,
    formState: { isValid, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      startTime: new Date(dateAndTimeToISO(activity.date, activity.startTime)),
      endTime: new Date(dateAndTimeToISO(activity.date, activity.endTime)),
      date: new Date(dateAndTimeToISO(activity.date)),
      pictogramId: activity.pictogramId,
    },
    mode: "onChange",
  });

  const onSubmit = async (formData: FormData) => {
    if (id === null) {
      addToast({ message: "Fejl, prøvede at tilføje aktivitet uden at vælge en borger", type: "error" });
      return;
    }

    const startTimeHHMM = formatTimeHHMM(formData.startTime);
    const endTimeHHMM = formatTimeHHMM(formData.endTime);
    const data: FullActivityDTO = {
      activityId: activity.activityId,
      citizenId: id,
      date: formData.date.toDateString(),
      startTime: startTimeHHMM,
      endTime: endTimeHHMM,
      isCompleted: activity.isCompleted,
      pictogramId: formData.pictogramId,
    };

    updateActivity
      .mutateAsync(data)
      .catch((error) => addToast({ message: (error as any).message, type: "error" }))
      .finally(() => router.back());
  };

  return (
    <SafeArea>
      <TouchableWithoutFeedback onPress={Keyboard.dismiss}>
        <ProgressSteps ref={progressRef}>
          <ScrollView contentContainerStyle={{ flexGrow: 1 }} bounces={false}>
            <FormContainer>
              <FormHeader title="Ændre Aktivitet" />
              <FormTimePicker control={control} name="startTime" placeholder="Vælg start tid" />
              <FormTimePicker control={control} name="endTime" placeholder="Vælg slut tid" />
              <FormTimePicker control={control} name="date" placeholder="Dato for aktivitet" mode="date" />
              <SecondaryButton
                style={{ backgroundColor: colors.green }}
                onPress={() => progressRef.current?.nextStep()}
                label={"Næste"}
              />
              <SecondaryButton onPress={() => router.back()} label={"Tilbage"} />
            </FormContainer>
          </ScrollView>
          <FormContainer style={{ paddingTop: 20 }}>
            {displayUrl ? (
              <Image
                source={{ uri: displayUrl }}
                style={{
                  width: ScaleSizeH(75),
                  height: ScaleSizeH(75),
                  position: "absolute",
                  top: ScaleSizeH(-60),
                  right: ScaleSizeW(10),
                }}
              />
            ) : null}
            <PictogramSelector
              organisationId={1}
              selectedPictogram={activity.pictogramId}
              setSelectedPictogram={(pictogram) => {
                setValue("pictogramId", pictogram.id, { shouldValidate: true });
                setPreviewUrl(pictogramImageUrl(pictogram.pictogramUrl));
              }}
            />
            <SubmitButton
              isValid={isValid}
              isSubmitting={isSubmitting}
              handleSubmit={handleSubmit(onSubmit)}
              label="Opdater aktivitet"
            />
            <SecondaryButton onPress={() => progressRef.current?.previousStep()} label={"Tilbage"} />
          </FormContainer>
        </ProgressSteps>
      </TouchableWithoutFeedback>
    </SafeArea>
  );
};

export default ActivityEdit;
